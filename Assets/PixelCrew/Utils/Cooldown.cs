using System;
using UnityEngine;

namespace PixelCrew.Utils
{
    [Serializable]
    public class Cooldown
    {
        [SerializeField] private float _value;

        private float _timeIsUp;
        public void Reset()
        {
            _timeIsUp = Time.time + _value;
        }

        public bool IsReady => _timeIsUp <= Time.time;
    }
}