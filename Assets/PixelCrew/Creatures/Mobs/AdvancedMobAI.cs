﻿using PixelCrew.Components.GameObjectBased;
using PixelCrew.Components.ColliderBased;
using PixelCrew.Creatures.Mobs.Patrolling;
using System.Collections;
using UnityEngine;
using PixelCrew.Components.Audio;
using PixelCrew.Utils;

namespace PixelCrew.Creatures.Mobs
{
    public class AdvancedMobAI : MonoBehaviour
    {
        [SerializeField] private LayerCheck _vision;
        [SerializeField] private LayerCheck _canAttack;

        [SerializeField] private float _alarmDelay = 2f;
        [SerializeField] private float _attackCooldown = 1f;
        [SerializeField] private float _missHeroCooldown = 2f;

        [Header("Summon")]
        [SerializeField] private Cooldown _summonCooldown;
        [SerializeField] private SpawnComponent _summonCreature;

        private IEnumerator _current;
        private GameObject _target;
        private CapsuleCollider2D _collider;

        private static readonly int IsDeadKey = Animator.StringToHash("is-dead");
        private static readonly int SummonKey = Animator.StringToHash("summon");

        private SpawnListComponent _particles;
        private Creature _creature;
        private Animator _animator;
        private bool _isDead;
        private Patrol _patrol;
        private PlaySoundsComponent _sounds;

        private void Awake()
        {
            _particles = GetComponent<SpawnListComponent>();
            _creature = GetComponent<Creature>();
            _animator = GetComponent<Animator>();
            _patrol = GetComponent<Patrol>();
            _collider = GetComponent<CapsuleCollider2D>();
            _sounds = GetComponent<PlaySoundsComponent>();
        }

        private void Start()
        {
            StartState(_patrol.DoPatrol());
        }

        public void OnSummon()
        {
            _summonCreature.Spawn();
        }

        public void OnHeroInVision(GameObject go)
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

        private void LookAtHero()
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
                    SetDirectionToTarget();
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
                //_particles.Spawn("BiteEffect");
                yield return new WaitForSeconds(_attackCooldown);
            }

            StartState(GoToHero());
        }

        private void SetDirectionToTarget()
        {
            var direction = GetDirectionToTarget();
            _creature.SetDirection(direction);
        }

        private Vector2 GetDirectionToTarget()
        {
            var direction = _target.transform.position - transform.position;
            direction.y = 0;
            return direction.normalized;
        }

        private void StartState(IEnumerator coroutine)
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
            _animator.SetBool(IsDeadKey, true);

            //_collider.direction = (CapsuleDirection2D)1;
            //_collider.size = new Vector2(_collider.size.y, _collider.size.x); 
            //_collider.offset = Vector2.zero;

            //_collider.gameObject.layer = 11;

            _creature.SetDirection(Vector2.zero);
            if (_current != null)
            {
                StopCoroutine(_current);
            }
        }
    }
}
