using PixelCrew.Model.Data;
using PixelCrew.UI.Widgets;
using UnityEngine;

namespace PixelCrew.UI.Options
{
    public class OptionsWindow : WindowAnimation
    {
        [SerializeField] private AudioSettingsWidget _music;
        [SerializeField] private AudioSettingsWidget _sounds;

        protected override void Start()
        {
            base.Start();

            _music.SetModel(GameSettings.I.Music);
            _sounds.SetModel(GameSettings.I.Sounds);
        }
    }
}
