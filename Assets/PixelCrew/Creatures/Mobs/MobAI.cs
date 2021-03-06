using PixelCrew.Components.GameObjectBased;
using PixelCrew.Components.ColliderBased;
using PixelCrew.Creatures.Mobs.Patrolling;
using System.Collections;
using UnityEngine;
using PixelCrew.Components.Audio;

namespace PixelCrew.Creatures.Mobs
{
    public class MobAI : MonoBehaviour
    {
        [SerializeField] protected LayerCheck _vision;
        [SerializeField] protected LayerCheck _canAttack;

        [SerializeField] protected float _alarmDelay = 2f;
        [SerializeField] protected float _attackCooldown = 1f;
        [SerializeField] protected float _missHeroCooldown = 2f;

        [SerializeField] protected float _horizontalTreshold = 0.2f;

        protected IEnumerator _current;
        protected GameObject _target;
        protected CapsuleCollider2D _collider;

        protected static readonly int IsDeadKey = Animator.StringToHash("is-dead");

        protected SpawnListComponent _particles;
        protected Creature _creature;
        protected Animator _animator;
        protected bool _isDead;
        protected Patrol _patrol;
        protected PlaySoundsComponent _sounds;

        protected virtual void Awake()
        {
            _particles = GetComponent<SpawnListComponent>();
            _creature = GetComponent<Creature>();
            _animator = GetComponent<Animator>();
            _patrol = GetComponent<Patrol>();
            _collider = GetComponent<CapsuleCollider2D>();
            _sounds = GetComponent<PlaySoundsComponent>();
        }

        protected virtual void Start()
        {
            StartState(_patrol.DoPatrol());
        }

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

        protected virtual void LookAtHero()
        {
            if (_isDead) return;

            var direction = GetDirectionToTarget();
            _creature.SetDirection(Vector2.zero);
            _creature.UpdateSpriteDirection(direction);
        }

        private IEnumerator GoToHero()
        {
            while (_vision.IsTouchingLayer)
            {
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

        protected virtual void SetDirectionToTarget()
        {
            var direction = GetDirectionToTarget();
            _creature.SetDirection(direction);
        }

        protected virtual Vector2 GetDirectionToTarget()
        {
            var direction = _target.transform.position - transform.position;
            direction.y = 0;
            return direction.normalized;
        }

        protected virtual void StartState(IEnumerator coroutine)
        {
            _creature.SetDirection(Vector2.zero);

            if (_current != null)
            {
                StopCoroutine(_current);
            }

            _current = coroutine;
            StartCoroutine(coroutine);
        }

        public void OnDie()
        {
            _sounds.Play("Die");
            _particles.Spawn("DeadMark");
            _isDead = true;
            _animator.SetBool(IsDeadKey, _isDead);

            _creature.SetDirection(Vector2.zero);
            if (_current != null)
            {
                StopCoroutine(_current);
            }
        }
    }
}
