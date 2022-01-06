using UnityEngine;

namespace PixelCrew
{
    [RequireComponent(typeof(SpriteRenderer))] 

    public class SpriteFade : MonoBehaviour
    {
        [SerializeField] private float _speed;
        private SpriteRenderer _renderer;
        
        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            var currentAlpha = _renderer.color.a;
            var alpha = Mathf.Lerp(currentAlpha, 0f, Time.deltaTime * _speed);
            _renderer.color = new Color(_renderer.color.r, _renderer.color.g, _renderer.color.b, alpha);
        }
    }
}
