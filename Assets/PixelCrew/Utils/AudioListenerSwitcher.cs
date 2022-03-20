using UnityEngine;

namespace PixelCrew.Utils
{
    public class AudioListenerSwitcher : MonoBehaviour
    {
        private AudioListener _listener;

        private void Start()
        {
            _listener = GetComponent<AudioListener>();
        }

        public void On() => _listener.enabled = true;

        public void Off() => _listener.enabled = false;
    }
}
