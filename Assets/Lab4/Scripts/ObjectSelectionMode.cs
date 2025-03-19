using TMPro;
using UnityEngine;
using Lean.Touch;

namespace Lab4
{
    public class ObjectSelectionMode : MonoBehaviour, IInteractionManagerMode
    {
        [SerializeField] private GameObject _ui;
        [SerializeField] private GameObject _descriptionPanel;
        [SerializeField] private TMP_Text _objectTitleText;
        [SerializeField] private TMP_Text _objectDescriptionText;
        [SerializeField] private GameObject[] scaleMarkers = new GameObject[3];

        private CreatedObject _selectedObject = null;
        private char _activeAxis = ' ';
        private float _initialScaleDistance;
        private Vector3 _initialScale;
        private bool isSwiping = false;
        private bool _isMovingObject = false;

        public void Activate()
        {
            _ui.SetActive(true);
            _descriptionPanel.SetActive(false);
            _selectedObject = null;
            isSwiping = false;
        }

        public void Deactivate()
        {
            _descriptionPanel.SetActive(false);
            _ui.SetActive(false);
            _selectedObject = null;
            HideScaleMarkers();
            isSwiping = false;
        }

        private void OnEnable()
        {
            LeanTouch.OnFingerSwipe += OnSwipeLeft;
        }

        private void OnDisable()
        {
            LeanTouch.OnFingerSwipe -= OnSwipeLeft;
        }

        private void OnSwipeLeft(LeanFinger finger)
        {
            if (_isMovingObject || Input.touchCount > 1) return;

            if ((finger.LastScreenPosition - finger.StartScreenPosition).x < 0)
            {
                isSwiping = true;
                ResetObjectToInitialState();
                Invoke(nameof(ResetSwipeFlag), 0.3f);
            }
        }

        private void ResetSwipeFlag()
        {
            isSwiping = false;
        }

        public void BackToDefaultScreen()
        {
            InteractionManager.Instance.SelectMode(0);
        }

        public void TouchInteraction(Touch[] touches)
        {
            if (isSwiping) return;

            // Обработка одного касания
            if (touches.Length == 1 && touches[0].phase == TouchPhase.Began)
            {
                TrySelectObject(touches[0].position);
                return;
            }

            if (touches.Length == 1)
            {
                MoveSelectedObject(touches[0]);

                if (touches[0].phase == TouchPhase.Ended)
                    _isMovingObject = false;
            }
            else if (touches.Length == 2)
            {
                if (touches[0].phase == TouchPhase.Moved || touches[1].phase == TouchPhase.Moved)
                {
                    ScaleObject(touches);
                }

                Touch secondTouch = (touches[0].deltaTime < touches[1].deltaTime) ? touches[0] : touches[1];

                if (secondTouch.phase == TouchPhase.Began)
                {
                    ShowScaleMarkers(secondTouch.position);
                }
                else if (secondTouch.phase == TouchPhase.Moved)
                {
                    if (_activeAxis != ' ')
                        return;

                    DetectAxisSelection(secondTouch);
                }
                else if (secondTouch.phase == TouchPhase.Ended)
                {
                    HideScaleMarkers();
                    _activeAxis = ' ';
                }
            }

            if (touches.Length != 2)
            {
                HideScaleMarkers();
                _activeAxis = ' ';
            }
        }

        private void TrySelectObject(Vector2 pos)
        {
            Ray ray = InteractionManager.Instance.ARCamera.ScreenPointToRay(pos);
            RaycastHit hitObject;

            if (!Physics.Raycast(ray, out hitObject))
                return;

            if (!hitObject.collider.CompareTag("CreatedObject"))
                return;

            GameObject selectedObject = hitObject.collider.gameObject;
            CreatedObject newSelectedObject = selectedObject.GetComponent<CreatedObject>();

            if (!newSelectedObject)
                throw new MissingComponentException("[OBJECT_SELECTION_MODE] " + selectedObject.name + " has no description!");

            // Выбираем новый объект
            _isMovingObject = true;
            _selectedObject = newSelectedObject;
            _selectedObject.SaveInitialState();
            ShowObjectDescription(_selectedObject);
        }

        private void ShowObjectDescription(CreatedObject targetObject)
        {
            _objectTitleText.text = targetObject.Name;
            _objectDescriptionText.text = targetObject.Description;
            _descriptionPanel.SetActive(true);
        }

        private void MoveSelectedObject(Touch touch)
        {
            if (touch.phase != TouchPhase.Moved || _isMovingObject == false)
                return;

            Debug.Log("[OBJECT_SELECTION_MODE] moved");
            _selectedObject.transform.position = InteractionManager.Instance.GetARRaycastHits(touch.position)[0].pose.position;

            //Ray ray = InteractionManager.Instance.ARCamera.ScreenPointToRay(touch.position);
            //RaycastHit[] hits = Physics.RaycastAll(ray);

            //foreach (RaycastHit hit in hits)
            //{
            //    Debug.Log("[OBJECT_SELECTION_MODE] " + hit.collider.tag);
            //    if (hit.collider.CompareTag("CreatedObject") &&
            //        hit.collider.gameObject == _selectedObject.gameObject)
            //    {
            //        _isMovingObject = true;
            //        Debug.Log("[OBJECT_SELECTION_MODE] moved");
            //        _selectedObject.transform.position = InteractionManager.Instance.GetARRaycastHits(touch.position)[0].pose.position;
            //        return;
            //    }
            //}

        }

        private void ResetObjectToInitialState()
        {
            if (_selectedObject != null)
            {
                Debug.Log("[OBJECT_SELECTION_MODE] reset");
                _selectedObject.ResetToInitialState();
            }
        }

        private void ShowScaleMarkers(Vector2 pos)
        {
            if (_selectedObject == null) return;

            scaleMarkers[0].transform.position = pos + Vector2.right * 200;
            scaleMarkers[1].transform.position = pos + Vector2.up * 200;
            scaleMarkers[2].transform.position = pos + Vector2.left * 100 * Mathf.Sqrt(2) + Vector2.down * 100 * Mathf.Sqrt(2);

            Vector2 touchScreenPosition = InteractionManager.Instance.ARCamera.WorldToScreenPoint(pos);
            Vector2 markerScreen0Position = InteractionManager.Instance.ARCamera.WorldToScreenPoint(scaleMarkers[0].transform.position);
            Vector2 markerScreen1Position = InteractionManager.Instance.ARCamera.WorldToScreenPoint(scaleMarkers[1].transform.position);
            Vector2 markerScreen2Position = InteractionManager.Instance.ARCamera.WorldToScreenPoint(scaleMarkers[2].transform.position);

            Debug.Log("[DETECTED LOG] TOUCH SCREEN POSITION " + pos + " SCREEN " + touchScreenPosition);
            Debug.Log("[DETECTED LOG] X POSITION " + scaleMarkers[0].transform.position + " SCREEN " + markerScreen0Position + " DISTANCE " + Vector2.Distance(touchScreenPosition, markerScreen0Position));
            Debug.Log("[DETECTED LOG] Y POSITION " + scaleMarkers[1].transform.position + " SCREEN " + markerScreen1Position + " DISTANCE " + Vector2.Distance(touchScreenPosition, markerScreen1Position));
            Debug.Log("[DETECTED LOG] Z POSITION " + scaleMarkers[2].transform.position + " SCREEN " + markerScreen2Position + " DISTANCE " + Vector2.Distance(touchScreenPosition, markerScreen2Position));

            foreach (var marker in scaleMarkers)
            {
                marker.SetActive(true);
            }
        }

        private void HideScaleMarkers()
        {
            foreach (var marker in scaleMarkers)
            {
                marker.SetActive(false);
            }
        }

        private void DetectAxisSelection(Touch touch)
        {
            foreach (var marker in scaleMarkers)
            {
                if (Vector2.Distance(touch.position, marker.transform.position) < 50)
                {
                    if (marker == scaleMarkers[0]) _activeAxis = 'X';
                    else if (marker == scaleMarkers[1]) _activeAxis = 'Y';
                    else if (marker == scaleMarkers[2]) _activeAxis = 'Z';

                    HideScaleMarkers();

                    _initialScaleDistance = Vector2.Distance(Input.touches[0].position, Input.touches[1].position);
                    _initialScale = _selectedObject.transform.localScale;

                    return;
                }
            }
        }

        private void ScaleObject(Touch[] touches)
        {
            if (_selectedObject == null || _activeAxis == ' ') return;

            float currentDistance = Vector2.Distance(touches[0].position, touches[1].position);

            float scaleFactor = currentDistance / _initialScaleDistance;

            Vector3 newScale = _selectedObject.transform.localScale;

            if (_activeAxis == 'X')
            {
                newScale.x = Mathf.Max(0.1f, _initialScale.x * scaleFactor);
            }
            else if (_activeAxis == 'Y')
            {
                newScale.y = Mathf.Max(0.1f, _initialScale.y * scaleFactor);
            }
            else if (_activeAxis == 'Z')
            {
                newScale.z = Mathf.Max(0.1f, _initialScale.z * scaleFactor);
            }

            _selectedObject.transform.localScale = newScale;
        }

        private void ClearSelection()
        {
            if (_selectedObject != null)
            {
                _selectedObject.transform.localScale = Vector3.one;

                HideScaleMarkers();

                _selectedObject = null;
            }

            _descriptionPanel.SetActive(false);
        }
    }
}
