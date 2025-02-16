using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Lab3
{
    public class ArrowIndicator : MonoBehaviour
    {
        public static ArrowIndicator Instance { get; private set; }

        [SerializeField] private GameObject[] _arrows; // ������� �������

        private Camera _mainCamera;
        private Dictionary<GameObject, GameObject> _activeArrows = new Dictionary<GameObject, GameObject>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            _mainCamera = Camera.main;

            if (_arrows == null || _arrows.Length == 0)
            {
                Debug.LogError("������ _arrows ���� ��� �� ��������!");
            }
        }

        private void Update()
        {
            var trackedObjects = ObjectTracker.Instance.GetCreatedObjects();

            // ������� ������� ��� ��������, ������� ������ �� �������������
            List<GameObject> objectsToRemove = new List<GameObject>();
            foreach (var tracked in _activeArrows.Keys)
            {
                if (!trackedObjects.ContainsKey(tracked))
                {
                    _activeArrows[tracked].SetActive(false);
                    objectsToRemove.Add(tracked);
                }
            }
            foreach (var obj in objectsToRemove)
            {
                _activeArrows.Remove(obj);
            }

            // ��������� ����������� �������
            foreach (var obj in trackedObjects.Keys)
            {
                if (!_activeArrows.ContainsKey(obj))
                {
                    ShowArrow(obj);
                }

                bool isVisible = ObjectTracker.Instance.IsObjectVisible(obj);
                if (!isVisible && !_activeArrows[obj].activeSelf)
                {
                    Debug.Log($"������ {obj.name} �������. ���������� �������.");
                    _activeArrows[obj].SetActive(true);
                }
                else if (isVisible && _activeArrows[obj].activeSelf)
                {
                    Debug.Log($"������ {obj.name} �����. �������� �������.");
                    _activeArrows[obj].SetActive(false);
                }

                UpdateArrowPosition(obj);
            }
        }

        private void ShowArrow(GameObject target)
        {
            GameObject arrow = GetArrowForObject(target);
            if (arrow == null)
            {
                Debug.LogError($"�� ������� ������� ��� {target.name}");
                return;
            }

            _activeArrows[target] = arrow;
            arrow.SetActive(true);

            UpdateArrowPosition(target);
        }

        private void UpdateArrowPosition(GameObject target)
        {
            if (!_activeArrows.ContainsKey(target)) return;

            Vector3 targetScreenPos = _mainCamera.WorldToScreenPoint(target.transform.position);
            Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
            Vector3 direction = (targetScreenPos - screenCenter).normalized;

            // ������������� �����������, ���� ������ �� �������
            if (targetScreenPos.z < 0)
            {
                direction = -direction;
            }

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // ��������� ������� ������
            float borderX = Screen.width / 2 * 0.9f;
            float borderY = Screen.height / 2 * 0.9f;

            float arrowX = borderX * Mathf.Sign(direction.x);
            float arrowY = arrowX * Mathf.Tan(angle * Mathf.Deg2Rad);

            if (Mathf.Abs(arrowY) > borderY)
            {
                arrowY = borderY * Mathf.Sign(direction.y);
                arrowX = arrowY / Mathf.Tan(angle * Mathf.Deg2Rad);
            }

            Vector3 arrowPos = screenCenter + new Vector3(arrowX, arrowY, 0);

            // ������������ ������� � �������� ������
            arrowPos.x = Mathf.Clamp(arrowPos.x, 0, Screen.width);
            arrowPos.y = Mathf.Clamp(arrowPos.y, 0, Screen.height);

            GameObject arrow = _activeArrows[target];
            arrow.transform.position = arrowPos;
            arrow.transform.rotation = Quaternion.Euler(0, 0, angle);
        }



        private GameObject GetArrowForObject(GameObject target)
        {
            if (target.CompareTag("RedCube"))
            {
                return _arrows.Length > 0 ? _arrows[0] : null;
            }
            else if (target.CompareTag("GreenSphere"))
            {
                return _arrows.Length > 1 ? _arrows[1] : null;
            }
            else if (target.CompareTag("BlueCylinder"))
            {
                return _arrows.Length > 2 ? _arrows[2] : null;
            }

            return null;
        }
    }
}