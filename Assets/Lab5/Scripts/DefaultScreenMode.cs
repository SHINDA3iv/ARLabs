using UnityEngine;

namespace Lab5
{
    public class DefaultScreenMode : MonoBehaviour, IInteractionManagerMode
    {
        [SerializeField] private GameObject _ui;

        public void Activate()
        {
            _ui.SetActive(true);
        }

        public void Deactivate()
        {
            _ui.SetActive(false);
        }

        public void TouchInteraction(Touch[] touches)
        {
            return;
        }

        public void SelectMode(int mode)
        {
            InteractionManager.Instance.SelectMode(mode);
        }
    }
}