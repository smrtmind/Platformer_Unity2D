using PixelCrew.Model;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PixelCrew.Components
{
    public class ReloadLevelComponent : MonoBehaviour
    {
        private Hero _hero;

        private void Start()
        {
            _hero = FindObjectOfType<Hero>();
        }

        public void Reload()
        {
            var session = FindObjectOfType<GameSession>();
            Destroy(session.gameObject);

            var scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);

            _hero.SetCoinsToDefault();
            Debug.Log("Replay level");
        }
    }
}
