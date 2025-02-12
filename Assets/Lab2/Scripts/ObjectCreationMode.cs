using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lab2
{
    public class ObjectCreationMode : MonoBehaviour, IInteractionManagerMode
    {
        [SerializeField] private GameObject[] _spawnedObjectPrefabs;
        [SerializeField] private Button[] _spawnButtons;
        [SerializeField] private GameObject _ui;
        [SerializeField] private GameObject _targetMarkerPrefab;

        private List<TextMeshProUGUI> _coordinateTexts = new List<TextMeshProUGUI>();
        private Dictionary<int, bool> _createdObjects = new Dictionary<int, bool>();

        private int _spawnedObjectType = -1;
        private GameObject _targetMarker;

        private void Start()
        {
            _targetMarker = Instantiate(
                original: _targetMarkerPrefab,
                position: Vector3.zero,
                rotation: _targetMarkerPrefab.transform.rotation
            );
            _targetMarker.SetActive(false);

            for (int i = 0; i < _spawnedObjectPrefabs.Length; i++)
            {
                _createdObjects[i] = false;

                TextMeshProUGUI text = CreateCoordinateText(_spawnButtons[i].transform);
                text.gameObject.SetActive(false);
                _coordinateTexts.Add(text);
            }
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
            if (_createdObjects.ContainsKey(_spawnedObjectType) && _createdObjects[_spawnedObjectType])
            {
                Debug.Log("Этот объект уже создан.");
                return;
            }

            GameObject spawnedObject = Instantiate(
                original: _spawnedObjectPrefabs[_spawnedObjectType],
                position: InteractionManager.Instance.GetARRaycastHits(touch.position)[0].pose.position,
                rotation: _spawnedObjectPrefabs[_spawnedObjectType].transform.rotation
            );

            ObjectTracker.Instance.RegisterObject(spawnedObject);

            DisableSpawnButton(_spawnedObjectType);
        }

        private void DisableSpawnButton(int objectType)
        {
            if (objectType >= 0 && objectType < _spawnButtons.Length)
            {
                _spawnButtons[objectType].gameObject.SetActive(false);

                if (_createdObjects.ContainsKey(objectType))
                {
                    _createdObjects[objectType] = true;
                }

                if (objectType < _coordinateTexts.Count)
                {
                    _coordinateTexts[objectType].gameObject.SetActive(true);
                }
            }
        }

        private TextMeshProUGUI CreateCoordinateText(Transform buttonTransform)
        {
            // Создаем новый объект для текста
            GameObject textObject = new GameObject("CoordinateText");

            // Получаем родительский объект кнопки (обычно это Content объект ScrollView)
            Transform parent = buttonTransform.parent;

            // Вставляем текстовый объект в ту же позицию в иерархии, где находится кнопка
            textObject.transform.SetParent(parent, false);
            textObject.transform.SetSiblingIndex(buttonTransform.GetSiblingIndex());

            // Копируем RectTransform параметры кнопки
            RectTransform buttonRect = buttonTransform.GetComponent<RectTransform>();
            RectTransform textRect = textObject.AddComponent<RectTransform>();
            textRect.anchorMin = buttonRect.anchorMin;
            textRect.anchorMax = buttonRect.anchorMax;
            textRect.pivot = buttonRect.pivot;
            textRect.anchoredPosition = buttonRect.anchoredPosition;
            textRect.sizeDelta = buttonRect.sizeDelta;

            // Добавляем компонент TextMeshProUGUI
            TextMeshProUGUI textComponent = textObject.AddComponent<TextMeshProUGUI>();
            textComponent.fontSize = 20;
            textComponent.color = Color.white;
            textComponent.alignment = TextAlignmentOptions.Center;
            textComponent.text = "Position: (0, 0, 0)";

            return textComponent;
        }

        public void ClearAllObjects()
        {
            foreach (var pair in _createdObjects.ToList())
            {
                int objectType = pair.Key;

                if (pair.Value)
                {
                    GameObject obj = ObjectTracker.Instance.GetCreatedObjects().Keys.FirstOrDefault(x => x.name.Contains(_spawnedObjectPrefabs[objectType].name));
                    if (obj != null)
                    {
                        ObjectTracker.Instance.UnregisterObject(obj);

                        Destroy(obj);
                    }

                    if (objectType >= 0 && objectType < _spawnButtons.Length)
                    {
                        _spawnButtons[objectType].gameObject.SetActive(true);
                    }

                    if (objectType < _coordinateTexts.Count)
                    {
                        _coordinateTexts[objectType].gameObject.SetActive(false);
                    }

                    _createdObjects[objectType] = false;
                }
            }

            _spawnedObjectType = -1;

            ShowMarker(false);
        }
    }
}
