using PixelCrew.Components.Health;
using PixelCrew.Components.ColliderBased;
using PixelCrew.Model;
using PixelCrew.Utils;
using UnityEditor.Animations;
using UnityEngine;

namespace PixelCrew.Creatures.Hero
{
    public class Hero : Creature
    {
        [SerializeField] private CheckCircleOverlap _interactionCheck;
        [SerializeField] private LayerCheck _wallCheck;
        [SerializeField] private LayerCheck _frontObjectsCheck;
        [SerializeField] private LayerCheck _platformCheck;

        [SerializeField] private float _slamDownVelocity;
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
        private RigidbodyConstraints2D _defaultConstraints;
        private CapsuleCollider2D _collider;

        private static readonly int OnWallKey = Animator.StringToHash("is-onTheWall");
        private static readonly int ThrowKey = Animator.StringToHash("throw");


        private GameSession _session;
        private float _defaultGravityScale;

        protected override void Awake()
        {
            base.Awake();

            _defaultGravityScale = Rigidbody.gravityScale;
            _defaultConstraints = Rigidbody.constraints;
            _collider = GetComponent<CapsuleCollider2D>();
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

            var moveToSameDirection = Direction.x * transform.lossyScale.x > 0;
            if (_session.Data.WallClimbIsActive)
            {
                if (_wallCheck.IsTouchingLayer && moveToSameDirection)
                {
                    _isOnWall = true;
                    Rigidbody.constraints = RigidbodyConstraints2D.FreezePositionY;
                    Rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
                    Rigidbody.gravityScale = 0;
                }

                else
                {
                    _isOnWall = false;
                    Rigidbody.constraints = _defaultConstraints;
                    Rigidbody.gravityScale = _defaultGravityScale;
                }

                Animator.SetBool(OnWallKey, _isOnWall);
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
                if (Direction.x != 0 && !_frontObjectsCheck.IsTouchingLayer)
                {
                    _particles.Spawn("DashWave");
                }

                Rigidbody.constraints = RigidbodyConstraints2D.FreezePositionY;
                Rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
                Rigidbody.constraints = _defaultConstraints;

                _dash = false;

                return xVelocity *= _dashForce;
            }

            return xVelocity;
        }

        protected override float CalculateYVelocity()
        {
            var yVelocity = Rigidbody.velocity.y;
            var isJumpPressing = Jump > 0;//_direction.y > 0
            var isDownArrayPressing = Direction.y < 0;

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

            if (isJumpPressing && isDownArrayPressing && _platformCheck.IsTouchingLayer)
            {
                _collider.enabled = false;
                Invoke("EnableHeroCollider", 0.4f);
            }

            return base.CalculateYVelocity();
        }

        private void EnableHeroCollider()
        {
            _collider.enabled = true;
        }

        protected override float CalculateJumpVelocity(float yVelocity)
        {
            if (!IsGrounded && !_isOnWall && _allowDoubleJump && _session.Data.DoubleJumpIsActive)
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
            _session.Data.WallClimbIsActive = true;
            Debug.Log("NEW SKILL: WALL CLIMB");
        }

        public void ActivateThrowingSword()
        {
            _session.Data.ThrowingSwords += 5;
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
            _session.Data.ThrowingSwords++;
            Debug.Log($"+{1} sword. TOTAL SWORDS: {_session.Data.ThrowingSwords}");
        } 

        public void OnDoThrow()
        {
            _particles.Spawn("Throw");
        }

        public void Throw()
        {
            if (_session.Data.ThrowingSwordIsActive && _session.Data.IsArmed)
            {
                if (_throwCooldown.IsReady && _session.Data.ThrowingSwords > 1)
                {
                    _session.Data.ThrowingSwords--;
                    Animator.SetTrigger(ThrowKey);
                    _throwCooldown.Reset();
                }               
            }
        }
    }
}