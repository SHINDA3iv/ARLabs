using System.Collections.Generic;
using UnityEngine;

namespace Lab2
{
    public class ArrowIndicator : MonoBehaviour
    {
        public static ArrowIndicator Instance { get; private set; }

        [SerializeField] private GameObject[] _arrowPrefabs; // Префабы стрелок для каждого типа
        private Dictionary<GameObject, GameObject> _arrows = new Dictionary<GameObject, GameObject>();

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

        private void Update()
        {
            var trackedObjects = ObjectTracker.Instance.GetCreatedObjects();

            foreach (var obj in trackedObjects.Keys)
            {
                if (!ObjectTracker.Instance.IsObjectVisible(obj))
                {
                    ShowArrow(obj);
                }
                else
                {
                    HideArrow(obj);
                }
            }
        }

        private void ShowArrow(GameObject target)
        {
            if (!_arrows.ContainsKey(target))
            {
                GameObject arrow = Instantiate(GetArrowPrefab(target), transform);
                _arrows[target] = arrow;
            }

            UpdateArrowPosition(target);
            _arrows[target].SetActive(true);
        }

        private void HideArrow(GameObject target)
        {
            if (_arrows.ContainsKey(target))
            {
                _arrows[target].SetActive(false);
            }
        }

        private void UpdateArrowPosition(GameObject target)
        {
            if (!_arrows.ContainsKey(target)) return;

            Vector3 targetScreenPos = _mainCamera.WorldToScreenPoint(target.transform.position);

            // Если объект позади камеры, направляем стрелку вперед
            if (targetScreenPos.z < 0)
            {
                targetScreenPos.x = Screen.width - targetScreenPos.x;
                targetScreenPos.y = Screen.height - targetScreenPos.y;
            }

            Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
            Vector3 direction = (targetScreenPos - screenCenter).normalized;

            // Ограничиваем стрелку краем экрана
            float arrowDistance = Screen.height / 2 * 0.9f;
            Vector3 arrowPos = screenCenter + direction * arrowDistance;

            _arrows[target].transform.position = arrowPos;
            _arrows[target].transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
        }

        private GameObject GetArrowPrefab(GameObject target)
        {
            if (target.CompareTag("RedCube"))
            {
                return _arrowPrefabs[0];
            }
            else if (target.CompareTag("BlueSphere"))
            {
                return _arrowPrefabs[1];
            }
            else if (target.CompareTag("GreenCylinder"))
            {
                return _arrowPrefabs[2];
            }

            return null;
        }
    }
}
