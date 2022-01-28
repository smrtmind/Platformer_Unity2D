﻿using PixelCrew.Components;
using PixelCrew.Model;
using PixelCrew.Utils;
using UnityEditor.Animations;
using UnityEngine;

namespace PixelCrew.Creatures
{
    public class Hero : Creature
    {
        [SerializeField] private CheckCircleOverlap _interactionCheck;
        [SerializeField] private LayerCheck _wallCheck;

        [SerializeField] private float _slamDownVelocity;
        [SerializeField] private float _interactionRadius;
        [SerializeField] private float _dashForce;
        [SerializeField] private float _maxSafeVelocity;

        [Header("Cooldowns")]
        [SerializeField] private Cooldown _throwCooldown;
        [SerializeField] private Cooldown _dashCooldown;

        [Space]
        [SerializeField] private AnimatorController _armed;
        [SerializeField] private AnimatorController _disArmed;

        [Header("Particles")]
        [SerializeField] private ParticleSystem _hitParticles;

        private bool _allowDoubleJump;
        private bool _isOnWall;
        private bool _dash;

        private static readonly int OnTheWallKey = Animator.StringToHash("is-onTheWall");
        private static readonly int ThrowKey = Animator.StringToHash("throw");


        private GameSession _session;
        private float _defaultGravityScale;

        protected override void Awake()
        {
            base.Awake();

            _defaultGravityScale = Rigidbody.gravityScale;
        }

        private void Start()
        {
            _session = FindObjectOfType<GameSession>();
            var health = GetComponent<HealthComponent>();

            health.SetHealth(_session.Data.Hp);
            UpdateHeroWeapon();
        }

        public void OnHealthChanged(int currentHealth)
        {
            _session.Data.Hp = currentHealth;
        }

        protected override void Update()
        {
            base.Update();

            if (_session.Data.WallHookIsActive)
            {
                if (_wallCheck.IsTouchingLayer && Direction.x == transform.localScale.x)
                {
                    _isOnWall = true;
                    Rigidbody.gravityScale = 0;

                    Animator.SetBool(OnTheWallKey, true);
                }

                else
                {
                    Animator.SetBool(OnTheWallKey, false);

                    _isOnWall = false;
                    Rigidbody.gravityScale = _defaultGravityScale;
                }
            }
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        public void Dash()
        {
            if (_session.Data.DashIsActive)
            {
                if (_dashCooldown.IsReady)
                {
                    _dash = true;
                    _dashCooldown.Reset();
                }
            }
        }

        public override void Attack()
        {
            if (!_session.Data.IsArmed) return;

            base.Attack();
            _particles.Spawn("SwordSlash");
        }

        protected override float CalculateXVelocity()
        {
            var xVelocity = base.CalculateXVelocity();

            if (_dash && _session.Data.DashIsActive)
            {
                if (Direction.x != 0)
                {
                    _particles.Spawn("DashWave");
                }

                Rigidbody.constraints = RigidbodyConstraints2D.FreezePositionY;
                Rigidbody.constraints = RigidbodyConstraints2D.None;
                Rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;

                _dash = false;

                return xVelocity *= _dashForce;
            }

            return xVelocity;
        }

        protected override float CalculateYVelocity()
        {
            var yVelocity = Rigidbody.velocity.y;
            var isJumpPressing = Jump > 0;//_direction.y > 0

            //check if you push up
            //if (_direction.y > 0)
            //{
            //    Debug.Log("PUSHED UP");
            //}

            if (IsGrounded || _isOnWall)
            {
                if (_session.Data.DoubleJumpIsActive)
                {
                    _allowDoubleJump = true;
                }
            }

            if (!isJumpPressing && _isOnWall)
            {
                return 0f;
            }

            return base.CalculateYVelocity();
        }

        protected override float CalculateJumpVelocity(float yVelocity)
        {
            if (!IsGrounded && _allowDoubleJump && _session.Data.DoubleJumpIsActive)
            {
                _particles.Spawn("Jump");
                _allowDoubleJump = false;

                return _jumpForce;
            }

            return base.CalculateJumpVelocity(yVelocity);
        }

        public void AddCoins(int coins)
        {
            _session.Data.Coins += coins;
            Debug.Log($"+{coins} coins. TOTAL COINS: {_session.Data.Coins}");
        }

        public override void TakeDamage()
        {
            base.TakeDamage();

            if (_session.Data.Coins > 0)
            {
                SpawnCoins();
            }
        }

        private void SpawnCoins()
        {
            var numCoinsToDispose = Mathf.Min(_session.Data.Coins, 5);
            _session.Data.Coins -= numCoinsToDispose;

            var burst = _hitParticles.emission.GetBurst(0);
            burst.count = numCoinsToDispose;
            _hitParticles.emission.SetBurst(0, burst);

            _hitParticles.Play();
        }

        public void Interact()
        {
            _interactionCheck.Check();
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.IsInLayer(_groundLayer))
            {
                var contact = other.contacts[0];
                if (contact.relativeVelocity.y >= _slamDownVelocity)
                {
                    _particles.Spawn("SlamDown");
                }

                if (contact.relativeVelocity.y >= _maxSafeVelocity)
                {
                    GetComponent<HealthComponent>().ModifyHealth(-1);
                }
            }
        }

        public void SetCoinsToDefault()
        {
            _session.Data.Coins = default;
        }

        public void ActivateDoubleJump()
        {
            _session.Data.DoubleJumpIsActive = true;
            Debug.Log("NEW SKILL: DOUBLE JUMP");
        }

        public void ActivateAirDash()
        {
            _session.Data.DashIsActive = true;
            Debug.Log("NEW SKILL: DASH");
        }

        public void ActivateWallHook()
        {
            _session.Data.WallHookIsActive = true;
            Debug.Log("NEW SKILL: HOOK (stick to walls)");
        }

        public void ActivateThrowingSword()
        {
            _session.Data.Swords += 4;
            _session.Data.ThrowingSwordIsActive = true;
            Debug.Log("NEW SKILL: THROWING SWORD");
        }

        public void ArmHero()
        {
            _session.Data.IsArmed = true;
            UpdateHeroWeapon();
        }

        private void UpdateHeroWeapon()
        {
            Animator.runtimeAnimatorController = _session.Data.IsArmed ? _armed : _disArmed;
        }

        public void AddSwords()
        {
            _session.Data.Swords++;
            Debug.Log($"+{1} sword. TOTAL SWORDS: {_session.Data.Swords}");
        } 

        public void OnDoThrow()
        {
            _particles.Spawn("Throw");
        }

        public void Throw()
        {
            if (_session.Data.ThrowingSwordIsActive && _session.Data.IsArmed)
            {
                if (_throwCooldown.IsReady && _session.Data.Swords > 1)
                {
                    _session.Data.Swords--;
                    Animator.SetTrigger(ThrowKey);
                    _throwCooldown.Reset();
                }               
            }
        }
    }
}