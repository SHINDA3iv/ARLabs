using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace Lab5
{
    public class FaceMaskMode : MonoBehaviour, IInteractionManagerMode
    {
        [SerializeField] private GameObject _ui;
        [SerializeField] private ARFaceManager _faceManager;

        public void Activate()
        {
            _ui.SetActive(true);
            if (InteractionManager.Instance.CurrentManagerMode != InteractionManager.ManagerMode.Faces)
                InteractionManager.Instance.SetManagerMode(InteractionManager.ManagerMode.Faces);
            foreach (ARFace face in _faceManager.trackables)
                face.gameObject.SetActive(true);

            InteractionManager.Instance.ShowFrontalCamera(true);
        }

        public void Deactivate()
        {
            _ui.SetActive(false);
            if (InteractionManager.Instance.CurrentManagerMode == InteractionManager.ManagerMode.Faces)
                InteractionManager.Instance.SetManagerMode(InteractionManager.ManagerMode.Planes);

            foreach (ARFace face in _faceManager.trackables)
                face.gameObject.SetActive(false);

            InteractionManager.Instance.ShowFrontalCamera(false);
        }

        public void TouchInteraction(Touch[] touches)
        {
            return;
        }

        public void BackToDefaultScreen()
        {
            InteractionManager.Instance.SelectMode(0);
        }
    }
}