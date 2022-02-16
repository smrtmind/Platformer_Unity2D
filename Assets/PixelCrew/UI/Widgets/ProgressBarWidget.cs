using UnityEngine;
using UnityEngine.UI;

namespace PixelCrew.UI.Widgets
{
    public class ProgressBarWidget : MonoBehaviour
    {
        //using slider
        [SerializeField] private Slider _bar;

        public void SetProgress(float progress)
        {
            _bar.value = progress;
        }

        //using image
        //[SerializeField] private Image _bar;

        //public void SetProgress(float progress)
        //{
        //    _bar.fillAmount = progress;
        //}
    }
}
