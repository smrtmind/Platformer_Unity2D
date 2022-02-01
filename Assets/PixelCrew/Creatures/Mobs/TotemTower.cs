using PixelCrew.Components.Health;
using PixelCrew.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PixelCrew.Creatures.Mobs
{
    public class TotemTower : MonoBehaviour
    {
        [SerializeField] private List<ShootingTrapAI> _traps;
        [SerializeField] private Cooldown _cooldown;

        private int _currentTrap;

        private void Start()
        {
            foreach (var shootinTrapAI in _traps)
            {
                shootinTrapAI.enabled = false;
                var hp = shootinTrapAI.GetComponent<HealthComponent>();
                hp._onDie.AddListener(() => OnTrapDead(shootinTrapAI));
            }
        }

        private void OnTrapDead(ShootingTrapAI shootinTrapAI)
        {
            var index = _traps.IndexOf(shootinTrapAI);
            _traps.Remove(shootinTrapAI);
            if (index < _currentTrap)
            {
                _currentTrap--;
            }
        }

        private void Update()
        {
            if (_traps.Count == 0)
            {
                enabled = false;
                Destroy(gameObject, 1f);
            }

            var hasAnyTarget = _traps.Any(x => x._vision.IsTouchingLayer);
            if (hasAnyTarget)
            {
                if (_cooldown.IsReady)
                {
                    _traps[_currentTrap].Shoot();
                    _cooldown.Reset();
                    _currentTrap = (int)Mathf.Repeat(_currentTrap + 1, _traps.Count);
                }
            }
        }
    }
}
