using UnityEngine;

namespace Lab2 { 
    public class AnimationBehaviour : MonoBehaviour
    {
        private Animator animator;

        void Start()
        {
            transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

            animator = GetComponent<Animator>();
            AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;

            int randomIndex = Random.Range(0, clips.Length);
            string randomClip = clips[randomIndex].name;

            animator.Play(randomClip);
        }
    }
}
