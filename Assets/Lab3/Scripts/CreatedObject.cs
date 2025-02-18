using System.Collections;
using UnityEngine;

namespace Lab3
{
    public class CreatedObject : MonoBehaviour
    {
        [SerializeField] private string _displayName;
        [SerializeField] private string _description;
        private int _number = -1;
        private Vector3 _originalScale;
        private Quaternion _originalRotation;
        private bool _isRotating = false;
        private float _existenceTime = 0f;

        private void Start()
        {
            _originalScale = transform.localScale;
            _originalRotation = transform.rotation;
        }

        private void Update()
        {
            _existenceTime += Time.deltaTime;

            if (_isRotating)
            {
                transform.Rotate(Vector3.up, 20 * Time.deltaTime);
            }
        }

        public string Name => _number >= 0 ? $"{_displayName} {_number}" : _displayName;

        public string Description => _description;

        public string Info => $"Существование: {Mathf.Round(_existenceTime)} сек\n" +
                              $"Координаты: {transform.position}\n" +
                              $"Поворот: {transform.rotation.eulerAngles}";

        public void GiveNumber(int number) => _number = number;

        public void Highlight()
        {
            StopAllCoroutines();
            StartCoroutine(ScaleObject(1.5f));
            _isRotating = true;
        }

        public void ResetObject()
        {
            StopAllCoroutines();
            StartCoroutine(ScaleObject(1f));
            transform.rotation = _originalRotation;
            _isRotating = false;
        }

        private IEnumerator ScaleObject(float targetScaleFactor)
        {
            Vector3 targetScale = _originalScale * targetScaleFactor;
            float duration = 0.5f;
            float elapsedTime = 0;

            while (elapsedTime < duration)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, targetScale, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            transform.localScale = targetScale;
        }
    }
}
