using System;
using UnityEngine;
using UnityEngine.Events;

namespace PixelCrew.Components
{
    public class EnterCollisionComponent : MonoBehaviour
    {
        [SerializeField] private string _tag;
        [SerializeField] private EnterCollisionEvent _action;

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag(_tag))
            {
                _action?.Invoke(other.gameObject);
            }
        } 
    }

    [Serializable]
    public class EnterCollisionEvent : UnityEvent<GameObject>
    {

    }
}
