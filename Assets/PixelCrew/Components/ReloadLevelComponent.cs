using UnityEngine;
using UnityEngine.SceneManagement;

namespace PixelCrew.Components
{
    public class ReloadLevelComponent : MonoBehaviour
    {
        [SerializeField] private Hero _hero;

        public void Reload()
        {
            var scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);

            _hero._coins = default;
            Debug.Log("Replay level");
        }
    }
}
