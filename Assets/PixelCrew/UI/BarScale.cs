using UnityEngine;

namespace PixelCrew.UI
{
    public class BarScale : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private GameObject _go;

        private Vector3 _scale;

        private void Start()
        {
            _scale = _go.transform.localScale;
        }

        private void Update()
        {
            if (_target.lossyScale.x < 0)
                _go.transform.localScale = new Vector3(-1 * _scale.x, _scale.y, _scale.z);
            else if (_target.lossyScale.x > 0)
                _go.transform.localScale = new Vector3(1 * _scale.x, _scale.y, _scale.z);
        }
    }
}
