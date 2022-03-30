using PixelCrew.Components.GameObjectBased;
using PixelCrew.Components.Health;
using PixelCrew.Utils;
using System.Collections;
using UnityEditor.Animations;
using UnityEngine;

namespace PixelCrew.Creatures.Mobs
{
    public class BossAI : MobAI
    {
        [Header("Throw")]
        [SerializeField] private Cooldown _throwCooldown;
        [SerializeField] private SpawnComponent _throwingWeapon;

        [Space]
        [SerializeField] private AnimatorController _phaseOne;
        [SerializeField] private AnimatorController _phaseTwo;
        [SerializeField] private int _healthForPhaseTwo;

        private static readonly int AgroKey = Animator.StringToHash("agro");
        private static readonly int DrinkKey = Animator.StringToHash("drink");
        private static readonly int ThrowKey = Animator.StringToHash("throw");

        private HealthComponent _health;

        protected override void Awake()
        {
            base.Awake();

            _health = GetComponent<HealthComponent>();

        }

        protected override void Start() => base.Start();

        public void OnThrow() => _throwingWeapon.Spawn();

        private void OnHeroInVision(GameObject go)
        {
            if (_isDead) return;

            _target = go;
            StartState(AgroToHero());
        }

        private IEnumerator AgroToHero()
        {
            LookAtHero();
            _particles.Spawn("Exclamation");
            yield return new WaitForSeconds(_alarmDelay);

            StartState(GoToHero());
        }

        protected override void LookAtHero() => base.LookAtHero();

        private IEnumerator GoToHero()
        {
            while (_vision.IsTouchingLayer)
            {
                if (_animator.runtimeAnimatorController == _phaseOne)
                {
                    if (_health.Health == _healthForPhaseTwo)
                    {
                        _animator.SetTrigger(DrinkKey);
                        _creature.SetDirection(Vector2.zero);
                        yield return new WaitForSeconds(2.5f);

                        _animator.runtimeAnimatorController = _phaseTwo;
                        _sounds.Play("Growl");
                        yield return new WaitForSeconds(2f);
                    }
                }

                if (_animator.runtimeAnimatorController == _phaseTwo)
                {
                    if (_throwCooldown.IsReady)
                    {
                        _throwCooldown.Reset();
                        _animator.SetTrigger(ThrowKey);
                        _creature.SetDirection(Vector2.zero);

                        yield return new WaitForSeconds(0.8f);
                    }
                }

                if (_canAttack.IsTouchingLayer)
                {
                    StartState(Attack());
                }

                else
                {
                    var horizontalDelta = Mathf.Abs(_target.transform.position.x - transform.position.x);
                    if (horizontalDelta <= _horizontalTreshold)
                    {
                        _creature.SetDirection(Vector2.zero);
                    }
                    else
                    {
                        SetDirectionToTarget();
                    }
                }

                yield return null;
            }

            _creature.SetDirection(Vector2.zero);
            _particles.Spawn("Miss");
            yield return new WaitForSeconds(_missHeroCooldown);

            StartState(_patrol.DoPatrol());
        }

        private IEnumerator Attack()
        {
            while (_canAttack.IsTouchingLayer)
            {
                _creature.Attack();
                yield return new WaitForSeconds(_attackCooldown);
            }

            StartState(GoToHero());
        }

        public void PlayVFX()
        {
            _sounds.Play("Hit");
            _particles.Spawn("BalalaikaSlash");
        }

        protected override void SetDirectionToTarget() => base.SetDirectionToTarget();

        protected override Vector2 GetDirectionToTarget() => base.GetDirectionToTarget();

        protected override void StartState(IEnumerator coroutine) => base.StartState(coroutine);
    }
}
