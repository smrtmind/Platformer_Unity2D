using UnityEngine;

namespace PixelCrew.Utils
{
    public class AudioUtils
    {
        public const string SoundsSourceTag = "SoundsAudioSource";

        public static AudioSource FindSfxSource()
        {
            return GameObject.FindWithTag(SoundsSourceTag).GetComponent<AudioSource>();
        }
    }
}
