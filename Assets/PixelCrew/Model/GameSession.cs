using PixelCrew.Model.Data;
using UnityEngine;

namespace PixelCrew.Model
{
    public class GameSession : MonoBehaviour
    {
        [SerializeField] private PlayerData _data;

        public PlayerData Data => _data;
        private PlayerData _save;

        private void Awake()
        {
            if (IsSessionExist())
            {
                Destroy(gameObject);
            }

            else
            {
                Save();
                DontDestroyOnLoad(this);
            }
        }

        private bool IsSessionExist()
        {
            var session = FindObjectOfType<GameSession>();
            if (session != this)
            {
                return true;
            }

            return false;

            //var sessions = FindObjectOfType<GameSession>();
            //foreach (var gameSession in sessions)
            //{
            //    if (gameSession != this)
            //    {
            //        return true;
            //    }
            //}

            //return false;
        }

        public void Save()
        {
            _save = _data.Clone();
        }

        public void LoadLastSave()
        {
            _data = _save.Clone();
        }
    }
}
