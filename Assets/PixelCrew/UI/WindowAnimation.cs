using UnityEngine;

namespace PixelCrew.UI
{
    public class WindowAnimation : MonoBehaviour
    {
        private Animator _animator;
        private static readonly int Show = Animator.StringToHash("show");
        private static readonly int Hide = Animator.StringToHash("hide");
        private static readonly int Escape = Animator.StringToHash("exit");
        private static readonly int LaunchGame = Animator.StringToHash("start");

        private void Start()
        {
            _animator = GetComponent<Animator>();

            _animator.SetTrigger(Show);
        }

        public void Close()
        {
            _animator.SetTrigger(Hide);
        }

        public void StartGame()
        {
            _animator.SetTrigger(LaunchGame);
        }

        public void Exit()
        {
            _animator.SetTrigger(Escape);
        }

        public virtual void OnCloseAnimationComplete()
        {
            Destroy(gameObject);
        }
    }
}
