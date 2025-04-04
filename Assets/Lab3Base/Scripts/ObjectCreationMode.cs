using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lab3Base
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
            GameObject newObject = Instantiate(
                original: _spawnedObjectPrefabs[_spawnedObjectType],
                position: InteractionManager.Instance.GetARRaycastHits(touch.position)[0].pose.position,
                rotation: _spawnedObjectPrefabs[_spawnedObjectType].transform.rotation
            );

            CreatedObject objectDesc = newObject.GetComponent<CreatedObject>();
            if (objectDesc == null)
            {
                throw new MissingComponentException(newObject.name);
            }

            objectDesc.GiveNumber(++_spawnedObjectCount);
        }
    }
}