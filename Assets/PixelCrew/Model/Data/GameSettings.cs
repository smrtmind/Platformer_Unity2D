using PixelCrew.Model.Data.Properties;
using UnityEngine;

namespace PixelCrew.Model.Data
{
    [CreateAssetMenu(menuName = "Data/GameSettings", fileName = "GameSettings")]

    public class GameSettings : ScriptableObject
    {
        [SerializeField] private FloatPersistentProperty _music;
        [SerializeField] private FloatPersistentProperty _sounds;

        public FloatPersistentProperty Music => _music;
        public FloatPersistentProperty Sounds => _sounds;

        private static GameSettings _instance;
        public static GameSettings I => _instance == null ? LoadGameSettings() : _instance;

        private static GameSettings LoadGameSettings()
        {
            return _instance = Resources.Load<GameSettings>("GameSettings");
        }

        private void OnEnable()
        {
            _music = new FloatPersistentProperty(1, SoundSettings.Music.ToString());
            _sounds = new FloatPersistentProperty(1, SoundSettings.Sounds.ToString());
        }

        private void OnValidate()
        {
            _music.Validate();
            _sounds.Validate();
        }
    }

    public enum SoundSettings
    {
        Music,
        Sounds
    }
}
