using TMPro;
using UnityEngine;
using Unity.VisualScripting;
using UnityEngine.XR.ARFoundation;

namespace Lab5
{
    public class ObjectSelectionMode : MonoBehaviour, IInteractionManagerMode
    {
        [Tooltip("UI objects to disable")]
        [SerializeField] private GameObject _ui;
        [SerializeField] private GameObject _descriptionPanel;
        [SerializeField] private TMP_Text _objectTitleText;
        [SerializeField] private TMP_Text _objectDescriptionText;

        private CreatedObject _selectedObject = null;
        Renderer _selectedObjectRenderer = null;
        private bool _needResetTouch = false;
        private bool _isMovingObject = false;

        public void Activate()
        {
            _ui.SetActive(true);
            _descriptionPanel.SetActive(false);
            _selectedObject = null;
        }

        public void Deactivate()
        {
            _descriptionPanel.SetActive(false);
            _ui.SetActive(false);
            _selectedObject = null;
        }

        public void BackToDefaultScreen()
        {
            InteractionManager.Instance.SelectMode(0);
        }

        public void TouchInteraction(Touch[] touches)
        {
            Touch touch = touches[0];
            bool overUI = touch.position.IsPointOverUIObject();

            if (_needResetTouch)
            {
                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    _needResetTouch = false;
                else
                    return;
            }

            if (touch.phase == TouchPhase.Began)
            {
                TrySelectObject(touch.position);
            }

            if (_selectedObject == null)
                return;

            if (touches.Length == 1)
            {
                MoveSelectedObject(touch);
            }
            else if (touches.Length == 2)
            {
                RotateSelectedObject(touch, touches[1]);
            }
        }

        private void TrySelectObject(Vector2 pos)
        {
            // fire a ray from camera to the target screen position
            Ray ray = InteractionManager.Instance.ARCamera.ScreenPointToRay(pos);
            RaycastHit hitObject;
            if (!Physics.Raycast(ray, out hitObject))
                return;

            if (!hitObject.collider.CompareTag("CreatedObject"))
            {
                ClearSelection();
                return;
            }

            // if we hit a spawned object tag, try to get info from it
            GameObject selectedObject = hitObject.collider.gameObject;
            _selectedObject = selectedObject.GetComponent<CreatedObject>();
            _selectedObjectRenderer = selectedObject.GetComponent<Renderer>();
            if (!_selectedObject)
                throw new MissingComponentException("[OBJECT_SELECTION_MODE] " + selectedObject.name + " has no description!");

            ShowObjectDescription(_selectedObject);

            _isMovingObject = true;
        }

        private void ShowObjectDescription(CreatedObject targetObject)
        {
            _objectTitleText.text = targetObject.Name;
            _objectDescriptionText.text = targetObject.Description;
            _descriptionPanel.SetActive(true);
        }

        private void MoveSelectedObject(Touch touch)
        {
            if (!_isMovingObject) return;

            if (touch.phase == TouchPhase.Began)
            {
                ARAnchor anchor = _selectedObject.GetComponent<ARAnchor>();

                if (anchor != null)
                    Destroy(anchor);
                return;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                _selectedObject.AddComponent<ARAnchor>();
                _isMovingObject = false;
                return;
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                Vector3 position = InteractionManager.Instance.GetARRaycastHits(touch.position)[0].pose.position;
                Collider platformCollider = GetPlatformUnderPosition(position, 5f);
                Debug.Log("[OBJECT_CREATION_MODE] " + platformCollider.transform);

                if (platformCollider == null)
                {
                    Debug.Log("[OBJECT_CREATION_MODE] Объект можно перемещать только на платформе!");
                    return;
                }

                if (_selectedObjectRenderer == null)
                {
                    Debug.LogError("[OBJECT_CREATION_MODE] Префаб объекта не имеет Renderer!");
                    return;
                }

                Bounds prefabBounds = _selectedObjectRenderer.bounds;

                Vector3 newPosition = platformCollider.ClosestPointOnBounds(position);
                newPosition.y += prefabBounds.extents.y;

                _selectedObject.transform.position = newPosition;
            }
        }

        private Collider GetPlatformUnderPosition(Vector3 position, float radius)
        {
            Collider[] hitColliders = Physics.OverlapSphere(position, radius);
            Collider nearestCollider = null;
            float minDistance = float.MaxValue;

            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Platform"))
                {
                    float distance = Vector3.Distance(position, hitCollider.ClosestPoint(position));
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearestCollider = hitCollider;
                    }
                }
            }

            return nearestCollider;
        }

        private void RotateSelectedObject(Touch touch1, Touch touch2)
        {
            if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
            {
                float distance = Vector2.Distance(touch1.position, touch2.position);
                float distancePrev = Vector2.Distance(touch1.position - touch1.deltaPosition, touch2.position - touch2.deltaPosition);
                float delta = distance - distancePrev;

                if (Mathf.Abs(delta) > 0.0f)
                    delta *= 0.1f;
                else
                    delta *= -0.1f;

                // when you want to rotate object by angle, multiply its quaternion-type rotation by a rotation angle quaternion
                _selectedObject.transform.rotation *= Quaternion.Euler(0.0f, delta, 0.0f);
            }
        }

        public void ClearSelection()
        {
            _selectedObject = null;
            _selectedObjectRenderer = null;
            _isMovingObject = false;
            _descriptionPanel.SetActive(false);
        }
    }
}
