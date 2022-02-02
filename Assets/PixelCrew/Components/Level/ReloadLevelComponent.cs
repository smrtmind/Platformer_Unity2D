using PixelCrew.Creatures.Hero;
using PixelCrew.Model;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PixelCrew.Components.Level
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
        }
    }
}
