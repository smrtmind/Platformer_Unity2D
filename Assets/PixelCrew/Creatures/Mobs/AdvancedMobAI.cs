using PixelCrew.Components.GameObjectBased;
using PixelCrew.Utils;
using System.Collections;
using UnityEngine;

namespace PixelCrew.Creatures.Mobs
{
    public class AdvancedMobAI : MobAI
    {
        [Header("Summon")]
        [SerializeField] private Cooldown _summonCooldown;
        [SerializeField] private SpawnComponent _summonCreature;

        private static readonly int SummonKey = Animator.StringToHash("summon");

        protected override void Awake() => base.Awake();

        protected override void Start() => base.Start();

        public void OnSummon() => _summonCreature.Spawn();

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
                if (_summonCooldown.IsReady)
                {
                    _summonCooldown.Reset();
                    _animator.SetTrigger(SummonKey);
                    _creature.SetDirection(Vector2.zero);

                    yield return new WaitForSeconds(1.5f);
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
                _sounds.Play("Hit");
                yield return new WaitForSeconds(_attackCooldown);
            }

            StartState(GoToHero());
        }

        protected override void SetDirectionToTarget() => base.SetDirectionToTarget();

        protected override Vector2 GetDirectionToTarget() => base.GetDirectionToTarget();

        protected override void StartState(IEnumerator coroutine) => base.StartState(coroutine);

        protected override void OnDie() => base.OnDie();
    }
}
