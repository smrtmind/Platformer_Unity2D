using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace PixelCrew.Components
{
    public class ProbabilityDropComponent : MonoBehaviour
    {
        [SerializeField] private int _count;
        [SerializeField] private DropData[] _drop;
        [SerializeField] private DropEvent _onDropCalculated;
        [SerializeField] private bool _spawnOnEnable;

        private void OnEnable()
        {
            if (_spawnOnEnable)
            {
                CalculateDrop(); 
            }
        }

        [ContextMenu("CalculateDrop")]
        public void CalculateDrop()
        {
            var itemsPerDrop = new GameObject[_count];
            var total = _drop.Sum(dropData => dropData.Probability);
            var sortedDrop = _drop.OrderBy(dropData => dropData.Probability);

            for (int i = 0; i < _count; i++)
            {
                var random = UnityEngine.Random.value * total;
                foreach (var dropData in sortedDrop)
                {
                    if (dropData.Probability >= random)
                    {
                        itemsPerDrop[i] = dropData.Drop;
                        break; 
                    }
                }
            }

            _onDropCalculated?.Invoke(itemsPerDrop);
        }

        [Serializable]
        public class DropData
        {
            public GameObject Drop;
            [Range(0f, 100f)] public float Probability;
        }

        [Serializable]
        public class DropEvent : UnityEvent<GameObject[]>
        {

        }
    }
}