using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Lab2
{
    [RequireComponent(typeof(ARRaycastManager))]
    public class InteractionManager : MonoBehaviour
    {
        [SerializeField] private GameObject _spawnedObjectPrefab;

        private ARRaycastManager _aRRaycastManager;
        private List<ARRaycastHit> _raycastHits;

        void Awake()
        {
            _aRRaycastManager = GetComponent<ARRaycastManager>();
            _raycastHits = new List<ARRaycastHit>();
        }

        void Update()
        {
            if (Input.touchCount > 0)
            {
                ProcessFirstTouch(Input.GetTouch(0));
            }
        }

        private void ProcessFirstTouch(Touch touch)
        {
            if (touch.phase == TouchPhase.Began)
            {
                SpawnObject(touch);
            }
        }

        private void SpawnObject(Touch touch) 
        {
            _aRRaycastManager.Raycast(touch.position, _raycastHits, TrackableType.Planes);
            Instantiate(_spawnedObjectPrefab, _raycastHits[0].pose.position, _spawnedObjectPrefab.transform.rotation);
        } 
    }
}