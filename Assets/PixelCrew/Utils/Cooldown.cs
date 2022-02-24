using System;
using UnityEngine;

namespace PixelCrew.Utils
{
    [Serializable]
    public class Cooldown
    {
        [SerializeField] private float _value;

        private float _timeIsUp;

        public float Value
        {
            get => _value;
            set => _value = value;
        }

        public void Reset()
        {
            _timeIsUp = Time.time + _value;
        }

        //public float TimeLasts => Mathf.Max(_timeIsUp - Time.deltaTime, 0);
        public bool IsReady => _timeIsUp <= Time.time;
    }
}