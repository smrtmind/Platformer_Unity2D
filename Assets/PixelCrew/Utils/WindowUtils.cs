using UnityEngine;
using UnityEngine.SceneManagement;

namespace PixelCrew.Utils
{
    public static class WindowUtils
    {
        public static void CreateWindow(string resourcePath)
        {
            var window = Resources.Load<GameObject>(resourcePath);
            var scene = SceneManager.GetActiveScene();
            Canvas canvas;

            if (scene.name == "MainMenu")
            {
                canvas = Object.FindObjectOfType<Canvas>();
            }
            else
            {
                canvas = GameObject.FindWithTag("MainCanvas").GetComponent<Canvas>();
            }

            Object.Instantiate(window, canvas.transform);         
        }
    }
}
