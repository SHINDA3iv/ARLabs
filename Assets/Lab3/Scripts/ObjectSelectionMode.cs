using UnityEngine;
using TMPro;

namespace Lab3
{
    public class ObjectSelectionMode : MonoBehaviour, IInteractionManagerMode
    {
        [SerializeField] private GameObject _ui;
        [SerializeField] private GameObject _descriptionPanel;
        [SerializeField] private TMP_Text _objectTitleText;
        [SerializeField] private TMP_Text _objectDescriptionText;

        private CreatedObject _selectedObject;

        public void Activate()
        {
            _ui.SetActive(true);
            _descriptionPanel.SetActive(false);
            _selectedObject = null;
        }

        public void Deactivate()
        {
            _descriptionPanel.SetActive(false);
            _ui.SetActive(false);
            DeselectCurrentObject();
        }

        public void BackToDefaultScreen()
        {
            InteractionManager.Instance.SelectMode(0);
        }

        public void TouchInteraction(Touch[] touches)
        {
            Touch touch = touches[0];
            bool overUI = touch.position.IsPointOverUIObject();

            if (touch.phase == TouchPhase.Began)
            {
                if (!overUI)
                {
                    TrySelectObject(touch.position);
                }
            }
        }

        private void TrySelectObject(Vector2 pos)
        {
            Ray ray = InteractionManager.Instance.ARCamera.ScreenPointToRay(pos);
            if (Physics.Raycast(ray, out RaycastHit hitObject))
                return;
            if (!hitObject.collider.CompareTag("CreatedObject"))
                return;

            GameObject selectedGameObject = hitObject.collider.gameObject;
            CreatedObject newObject = selectedGameObject.GetComponent<CreatedObject>();
            if (!newObject)
                throw new MissingComponentException($"[OBJECT_SELECTION_MODE] {selectedGameObject.name} не имеет компонента CreatedObject!");

            if (_selectedObject != newObject)
            {
                DeselectCurrentObject();
                _selectedObject = newObject;
                ShowObjectDescription(newObject);
            }
        }

        private void ShowObjectDescription(CreatedObject targetObject)
        {
            _objectTitleText.text = targetObject.Name;
            _descriptionPanel.SetActive(true);
            targetObject.Highlight();
        }

        public void CloseDescription()
        {
            _descriptionPanel.SetActive(false);
            DeselectCurrentObject();
        }

        private void DeselectCurrentObject()
        {
            if (_selectedObject != null)
            {
                _selectedObject.ResetObject();
                _selectedObject = null;
            }
        }

        private void Update()
        {
            if (_selectedObject != null && _descriptionPanel.activeSelf)
            {
                _objectDescriptionText.text = _selectedObject.Info;
            }
        }
    }
}
