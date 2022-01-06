using UnityEngine;

namespace PixelCrew.Components
{
    public class TeleportComponent : MonoBehaviour
    {
        [SerializeField] private Transform _destinationTransform;

        public void Teleport(GameObject target)
        {
            target.transform.position = _destinationTransform.position;
        }
    }
}
