using UnityEngine;

namespace PixelCrew.Components
{
    public class DamageComponent : MonoBehaviour
    {
        [SerializeField] private bool _isDamaging;
        [SerializeField] private int _damage;
        [SerializeField] private bool _isHealing;
        [SerializeField] private int _heal;

        private HealthComponent _healthComponent;

        public void ApplyDamage(GameObject target)
        {
            Target(target);
            if (_isDamaging && !_isHealing)
            {
                _healthComponent.ApplyDamage(_damage);
            }
        }

        public void ApplyHeal(GameObject target)
        {
            Target(target);
            if (_isHealing && !_isDamaging)
            {
                _healthComponent.ApplyHeal(_heal);
            }
        }

        private HealthComponent Target(GameObject target)
        {
            _healthComponent = target.GetComponent<HealthComponent>();

            if (_healthComponent != null)
            {
                return _healthComponent;
            }

            else
            {
                throw new System.Exception();
            }
        }
    }
}
