using PixelCrew.Model;
using PixelCrew.Model.Definitions;
using PixelCrew.UI.Widgets;
using PixelCrew.Utils;
using UnityEngine;

namespace PixelCrew.UI.Hud
{
    public class HudController : MonoBehaviour
    {
        [SerializeField] private ProgressBarWidget _healthBar;

        private GameSession _session;

        private bool _pauseIsAvailable;
        public bool PauseIsAvailable => _pauseIsAvailable;

        private void Start()
        {
            _session = FindObjectOfType<GameSession>();
            _session.Data.Hp.OnChanged += OnHealthChanged;

            OnHealthChanged(_session.Data.Hp.Value, 0);

            _pauseIsAvailable = true;
        }

        private void OnHealthChanged(int newValue, int oldValue)
        {
            var maxHealth = DefinitionsFacade.Instance.Player.MaxHealth;
            var value = (float)newValue / maxHealth;
            _healthBar.SetProgress(value);
        }

        public void OnSettings()
        {
            WindowUtils.CreateWindow("UI/InGameMenuWindow");
            _pauseIsAvailable = false;
        }

        public void ResetPauseAccess()
        {
            _pauseIsAvailable = true;
        }

        private void OnDestroy()
        {
            _session.Data.Hp.OnChanged -= OnHealthChanged;
        }
    }
}
