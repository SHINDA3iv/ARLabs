using System.Collections.Generic;
using UnityEngine;

namespace Lab2
{
    public class ObjectTracker : MonoBehaviour
    {
        public static ObjectTracker Instance { get; private set; }

        private Dictionary<GameObject, Vector3> _createdObjects = new Dictionary<GameObject, Vector3>();
        private Camera _mainCamera;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            _mainCamera = Camera.main;
        }

        public void RegisterObject(GameObject obj)
        {
            if (!_createdObjects.ContainsKey(obj))
            {
                _createdObjects.Add(obj, obj.transform.position);
            }
        }

        public void UnregisterObject(GameObject obj)
        {
            if (_createdObjects.ContainsKey(obj))
            {
                _createdObjects.Remove(obj);
            }
        }

        public bool IsObjectVisible(GameObject obj)
        {
            if (_mainCamera == null || !_createdObjects.ContainsKey(obj))
            {
                return false;
            }

            Vector3 viewportPosition = _mainCamera.WorldToViewportPoint(obj.transform.position);
            return viewportPosition.x >= 0 && viewportPosition.x <= 1 &&
                   viewportPosition.y >= 0 && viewportPosition.y <= 1 &&
                   viewportPosition.z > 0;
        }

        public Dictionary<GameObject, Vector3> GetCreatedObjects()
        {
            return _createdObjects;
        }
    }
}