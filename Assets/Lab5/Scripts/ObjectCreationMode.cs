using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Linq;

namespace Lab5
{
    public class ObjectCreationMode : MonoBehaviour, IInteractionManagerMode
    {
        [SerializeField] private GameObject[] _spawnedObjectPrefabs;
        [SerializeField] private GameObject _ui;
        [SerializeField] private GameObject _targetMarkerPrefab;

        private int _spawnedObjectType = -1;
        private int _spawnedObjectCount = 0;
        private GameObject _targetMarker;

        private void Start()
        {
            // create target marker
            _targetMarker = Instantiate(
                original: _targetMarkerPrefab,
                position: Vector3.zero,
                rotation: _targetMarkerPrefab.transform.rotation
            );
            _targetMarker.SetActive(false);
        }

        public void Activate()
        {
            _ui.SetActive(true);
            _spawnedObjectType = -1;
        }

        public void Deactivate()
        {
            _ui.SetActive(false);
            _spawnedObjectType = -1;
        }

        public void BackToDefaultScreen()
        {
            InteractionManager.Instance.SelectMode(0);
        }

        public void SetSpawnedObjectType(int spawnedObjectType)
        {
            _spawnedObjectType = spawnedObjectType;
        }

        public void TouchInteraction(Touch[] touches)
        {
            // if none are yet selected, return
            if (_spawnedObjectType == -1)
                return;

            Touch touch = touches[0];
            bool overUI = touch.position.IsPointOverUIObject();

            if (touch.phase == TouchPhase.Began)
            {
                if (!overUI)
                {
                    ShowMarker(true);
                    MoveMarker(touch.position);
                }
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                if (_targetMarker.activeSelf)
                    MoveMarker(touch.position);
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                if (_targetMarker.activeSelf)
                {
                    SpawnObject(touch);
                    ShowMarker(false);
                }
            }
        }
        private void ShowMarker(bool value)
        {
            _targetMarker.SetActive(value);
        }

        private void MoveMarker(Vector2 touchPosition)
        {
            _targetMarker.transform.position = InteractionManager.Instance.GetARRaycastHits(touchPosition)[0].pose.position;
        }

        private void SpawnObject(Touch touch)
        {
            Vector3 initialSpawnPosition = InteractionManager.Instance.GetARRaycastHits(touch.position)[0].pose.position;

            Vector3 spawnPosition = FindPlatformBelow(initialSpawnPosition);
            if (spawnPosition == Vector3.zero)
            {
                Debug.Log("[OBJECT_CREATION_MODE] Платформа не найдена под точкой касания!");
                return;
            }

            Collider platformCollider = GetPlatformUnderPosition(spawnPosition);
            if (platformCollider == null)
            {
                Debug.Log("[OBJECT_CREATION_MODE] Объект можно создавать только на платформе!");
                return;
            }

            GameObject prefabToSpawn = _spawnedObjectPrefabs[_spawnedObjectType];
            Renderer prefabRenderer = prefabToSpawn.GetComponent<Renderer>();

            if (prefabRenderer == null)
            {
                Debug.LogError("[OBJECT_CREATION_MODE] Префаб объекта не имеет Renderer!");
                return;
            }

            Bounds prefabBounds = prefabRenderer.bounds;

            // Вместо того чтобы поднимать объект на его высоту, просто касаемся платформы.
            Vector3 adjustedPosition = spawnPosition;
            adjustedPosition.y = platformCollider.ClosestPointOnBounds(spawnPosition).y + prefabBounds.extents.y;

            GameObject newObject = Instantiate(prefabToSpawn, adjustedPosition, prefabToSpawn.transform.rotation);

            CreatedObject objectDescription = newObject.GetComponent<CreatedObject>();
            if (objectDescription == null)
                throw new MissingComponentException("[OBJECT_CREATION_MODE] " + newObject.name + " missing CreatedObject!");
            objectDescription.GiveNumber(++_spawnedObjectCount);

            newObject.AddComponent<ARAnchor>();
        }

        private Vector3 FindPlatformBelow(Vector3 startPosition)
        {
            float maxDistance = 5.0f;
            RaycastHit[] hitsDown = Physics.RaycastAll(startPosition, Vector3.down, maxDistance);
            RaycastHit[] hitsUp = Physics.RaycastAll(startPosition, Vector3.up, maxDistance);

            RaycastHit[] hits = hitsDown.Concat(hitsUp).ToArray();

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.CompareTag("Platform"))
                {
                    return hit.point;
                }
            }

            return startPosition;
        }

        private Collider GetPlatformUnderPosition(Vector3 position)
        {
            Collider[] hitColliders = Physics.OverlapSphere(position, 0.05f);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Platform"))
                {
                    return hitCollider;
                }
            }
            return null;
        }
    }
}