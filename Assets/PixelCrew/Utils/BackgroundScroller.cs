using UnityEngine;

namespace PixelCrew.Utils
{
    public class BackgroundScroller : MonoBehaviour
    {
        [Range(-1f, 1f)] [SerializeField] private float _scrollingSpeed = 0.5f;

        private float _offset;
        private Material _material;

        private void Start()
        {
            _material = GetComponent<Renderer>().material;
        }

        private void Update()
        {
            _offset = (Time.deltaTime * _scrollingSpeed) / 10f;
            _material.SetTextureOffset("_MainTex", new Vector2(_offset, 0));
        }
    }
}
