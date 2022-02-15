using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PixelCrew.UI.MainMenu
{
    public class MainMenuWindow : WindowAnimation
    {
        private Action _closeAction;

        public void OnStartGame()
        {
            _closeAction = () => { SceneManager.LoadScene("LVL_1"); };
            Close();
        }

        public void OnShowOptions()
        {
            var window = Resources.Load<GameObject>("UI/OptionsWindow");
            var canvas = FindObjectOfType<Canvas>();
            Instantiate(window, canvas.transform);
        }

        public void OnLanguageOptions()
        {
            var window = Resources.Load<GameObject>("UI/LanguageWindow");
            var canvas = FindObjectOfType<Canvas>();
            Instantiate(window, canvas.transform);
        }

        public void OnExit()
        {
            _closeAction = () => 
            {
                Application.Quit();

#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            };
            Exit();
        }

        public override void OnCloseAnimationComplete()
        {
            _closeAction?.Invoke();
            base.OnCloseAnimationComplete();
        }
    }
}
