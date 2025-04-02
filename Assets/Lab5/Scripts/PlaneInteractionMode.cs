using System.Collections.Generic;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Lab5
{
    public class PlaneInteractionMode : MonoBehaviour, IInteractionManagerMode
    {
        [SerializeField] private GameObject _ui;
        [SerializeField] private GameObject _titlePanel;
        [SerializeField] private GameObject _platformPrefab;
        [SerializeField] private TextMeshProUGUI _tmpText;

        private List<Collider> _spawnedPlatforms = new List<Collider>();
        private ARPlaneManager _planeManager;
        private ARAnchorManager _anchorManager;

        private const float REQUIRED_PLANE_SIZE = 0.5f; // Минимальный размер плоскости

        private void Start()
        {
            _planeManager = FindObjectOfType<ARPlaneManager>();
            _anchorManager = FindObjectOfType<ARAnchorManager>();

            if (_planeManager == null)
                throw new MissingComponentException("[PLANE_INTERACTION_MODE] ARPlaneManager not found!");
        }

        public void Activate()
        {
            _ui.SetActive(true);
            _titlePanel.SetActive(false);
            UpdateHintText();
        }

        public void Deactivate()
        {
            _ui.SetActive(false);
            _titlePanel.SetActive(false);
        }

        public void TouchInteraction(Touch[] touches)
        {
            if (touches.Length == 0 || touches[0].phase != TouchPhase.Began)
            {
                Debug.Log("[PLANE_INTERACTION_MODE] Нет касания или оно не в фазе начала.");
                return;
            }

            Vector2 touchPosition = touches[0].position;
            List<ARRaycastHit> hits = InteractionManager.Instance.GetARRaycastHits(touchPosition, TrackableType.PlaneWithinBounds);

            if (hits.Count <= 0)
            {
                Debug.Log("[PLANE_INTERACTION_MODE] Касание не попало на плоскость.");
                return;
            }

            ARPlane plane = hits[0].trackable as ARPlane;
            if (plane == null)
            {
                Debug.Log("[PLANE_INTERACTION_MODE] ARPlane не найден.");
                return;
            }

            if (!IsPlaneLargeEnough(plane))
            {
                Debug.Log($"[PLANE_INTERACTION_MODE] Плоскость слишком мала: {plane.size.x} x {plane.size.y}");
                return;
            }

            Vector3 position = hits[0].pose.position;

            if (!CanPlacePlatform(plane, position))
                return;
            
            bool overUI = touchPosition.IsPointOverUIObject();
            if (overUI)
            {
                Debug.Log("[PLANE_INTERACTION_MODE] Касание произошло над UI.");
                return;
            }

            Debug.Log($"[PLANE_INTERACTION_MODE] Создаём платформу на позиции: {position}");
            SpawnPlatform(plane, position);
            UpdateHintText();
        }


        private bool IsPlaneLargeEnough(ARPlane plane)
        {
            return plane.size.x >= REQUIRED_PLANE_SIZE && plane.size.y >= REQUIRED_PLANE_SIZE;
        }

        private void SpawnPlatform(ARPlane plane, Vector3 position)
        {
            GameObject newPlatform = Instantiate(_platformPrefab, position, Quaternion.identity);
            newPlatform.AddComponent<ARAnchor>();

            _spawnedPlatforms.Add(newPlatform.GetComponent<Collider>());
        }

        private bool CanPlacePlatform(ARPlane plane, Vector3 position)
        {
            Vector2 planeSize = plane.size;
            Vector3 platformSize = _platformPrefab.GetComponent<Renderer>().bounds.size;

            float halfWidth = platformSize.x / 2;
            float halfHeight = platformSize.z / 2;

            Vector3 localPos = plane.transform.InverseTransformPoint(position);

            if (Mathf.Abs(localPos.x) + halfWidth > planeSize.x / 2 ||
                Mathf.Abs(localPos.z) + halfHeight > planeSize.y / 2)
            {
                Debug.Log("[PLANE_INTERACTION_MODE] Платформа выходит за границы плоскости!");
                return false;
            }

            Renderer platformRenderer = _platformPrefab.GetComponent<Renderer>();
            Vector3 size = platformRenderer.bounds.size;
            Bounds objBounds = new Bounds(position, new Vector3(0.5f, 0.01f, 0.5f));

            foreach (Collider platform in _spawnedPlatforms)
            {
                if (IsPlatformOnPlatform(objBounds, position, platform))
                {
                    Debug.Log("[PLANE_INTERACTION_MODE] Платформа не может быть размещена: слишком близко к другой.");
                    return false;
                }
            }
            return true;
        }

        private bool IsPlatformOnPlatform(Bounds objBounds, Vector3 position, Collider platformCollider)
        {
            Bounds platformBounds = platformCollider.bounds;

            return objBounds.Intersects(platformBounds);
        }

        private void UpdateHintText()
        {
            bool hasPlanes = _planeManager.trackables.count > 0;
            _titlePanel.SetActive(hasPlanes);
        }

        public void BackToDefaultScreen()
        {
            InteractionManager.Instance.SelectMode(0);
        }
    }
}