using UnityEngine;

namespace PixelCrew.Components
{
    public class HealthImpactComponent : MonoBehaviour
    {
        [SerializeField] private string _effect;
        [SerializeField] private int _amountOfPoints;

        private HealthComponent _healthComponent;

        public void ApplyOperation(GameObject target)
        {
            _healthComponent = target.GetComponent<HealthComponent>();
            if (_healthComponent != null)
            {
                switch (_effect.ToLower())
                {
                    case "damage":
                        _healthComponent.ApplyDamage(_amountOfPoints);
                        break;

                    case "heal":
                        _healthComponent.ApplyHeal(_amountOfPoints);
                        break;
                }
            }
        }

        //public void ApplyDamage(GameObject target)
        //{
        //    Target(target);
        //    _healthComponent.ApplyDamage(_amountOfPoints);
        //}

        //public void ApplyHeal(GameObject target)
        //{
        //    Target(target);
        //    _healthComponent.ApplyHeal(_amountOfPoints);
        //}

        //private HealthComponent Target(GameObject target)
        //{
        //    _healthComponent = target.GetComponent<HealthComponent>();

        //    if (_healthComponent != null)
        //    {
        //        return _healthComponent;
        //    }

        //    else
        //    {
        //        throw new System.Exception();
        //    }
        //}
    }
}
