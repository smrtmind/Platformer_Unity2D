using PixelCrew.Model;
using PixelCrew.UI.Hud;
using PixelCrew.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PixelCrew.UI.InGameMenu
{
    public class InGameMenuWindow : WindowAnimation
    {
        private float _defaultTimeScale;
        private HudController _hud;

        protected override void Start()
        {
            base.Start();

            _defaultTimeScale = Time.timeScale;
            Time.timeScale = 0;

            _hud = FindObjectOfType<HudController>();
        }

        public void OnShowSettings()
        {
            WindowUtils.CreateWindow("UI/OptionsWindow");
        }

        public void OnExit()
        {
            SceneManager.LoadScene("MainMenu");

            var session = FindObjectOfType<GameSession>();
            Destroy(session.gameObject);
        }

        private void OnDestroy()
        {
            Time.timeScale = _defaultTimeScale;
            _hud.ResetPauseAccess();
        }
    }
}
