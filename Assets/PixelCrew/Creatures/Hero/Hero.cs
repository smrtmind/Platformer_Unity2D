using PixelCrew.Components;
using PixelCrew.Components.ColliderBased;
using PixelCrew.Components.Health;
using PixelCrew.Model;
using PixelCrew.Model.Data;
using PixelCrew.Utils;
using System.Collections;
using UnityEditor.Animations;
using UnityEngine;

namespace PixelCrew.Creatures.Hero
{
    public class Hero : Creature, ICanAddToInventory
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

        [Header("Super Throw")]
        [SerializeField] private Cooldown _superThrowCooldown;
        [SerializeField] private int _superThrowParticles;
        [SerializeField] private float _superThrowDelay;

        [Space]
        [SerializeField] private AnimatorController _armed;
        [SerializeField] private AnimatorController _disArmed;
        [SerializeField] private ProbabilityDropComponent _hitDrop;

        private bool _superThrow;
        private bool _allowDoubleJump;
        private bool _isOnWall;
        private bool _dash;
        private RigidbodyConstraints2D _defaultConstraints;
        private CapsuleCollider2D _collider;

        private static readonly int OnWallKey = Animator.StringToHash("is-onTheWall");
        private static readonly int ThrowKey = Animator.StringToHash("throw");
        private static readonly int RunNearWallKey = Animator.StringToHash("is-runNearWall");
        private static readonly int IdleNearWallKey = Animator.StringToHash("is-idleNearWall");


        private GameSession _session;
        private HealthComponent _health;
        private float _defaultGravityScale;

        private int CoinsCount => _session.Data.Inventory.Count("Coin");
        private int ThrowingSwordCount => _session.Data.Inventory.Count("ThrowingSword");
        private int PotionsCount => _session.Data.Inventory.Count("HealthPotion");

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
            _health = GetComponent<HealthComponent>();
            _session.Data.Inventory.OnChanged += OnInventoryChange;
            _session.Data.Inventory.OnChanged += InventoryInfo;

            _health.SetHealth(_session.Data.Hp);
            UpdateHeroWeapon();
        }

        private void OnDestroy()
        {
            _session.Data.Inventory.OnChanged -= OnInventoryChange;
            _session.Data.Inventory.OnChanged -= InventoryInfo;
        }

        private void InventoryInfo(string id, int value)
        {
            Debug.Log($"Inventory changed: {id}: {value}");
        }

        private void OnInventoryChange(string id, int value)
        {
            if (id == "ThrowingSword")
            {
                UpdateHeroWeapon();
            }
        }

        public void OnHealthChanged(int currentHealth)
        {
            _session.Data.Hp = currentHealth;
        }

        protected override void Update()
        {
            base.Update();

            if (Animator.runtimeAnimatorController == _armed)
            {
                bool _isRunNearWall = _wallCheck.IsTouchingLayer && IsGrounded && _session.Data.SwordIsActive && Direction.x != 0 ? true : false;

                Animator.SetBool(RunNearWallKey, _isRunNearWall);
                Animator.SetBool(IdleNearWallKey, !_isRunNearWall);
            }

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
            if (!_session.Data.SwordIsActive) return;

            base.Attack();

            if (!_wallCheck.IsTouchingLayer)
            {
                _particles.Spawn("SwordSlash");
                Sounds.Play("Melee");
            }
        }

        protected override float CalculateXVelocity()
        {
            var xVelocity = base.CalculateXVelocity();

            if (_dash && _session.Data.DashIsActive)
            {
                if (Direction.x != 0 && !_frontObjectsCheck.IsTouchingLayer)
                {
                    _particles.Spawn("DashWave");
                    Sounds.Play("Dash");
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
                _allowDoubleJump = false;
                DoJumpVfx();

                return _jumpForce;
            }

            return base.CalculateJumpVelocity(yVelocity);
        }

        public void AddToInventory(string id, int value)
        {
            _session.Data.Inventory.Add(id, value);
        }

        public override void TakeDamage()
        {
            base.TakeDamage();
            if (CoinsCount > 0)
            {
                SpawnCoins();
            }
        }

        private void SpawnCoins()
        {
            var numCoinsToDispose = Mathf.Min(CoinsCount, 5);
            _session.Data.Inventory.Remove("Coin", numCoinsToDispose);

            _hitDrop.SetCount(numCoinsToDispose);
            _hitDrop.CalculateDrop();
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

        private void UpdateHeroWeapon()
        {
            Animator.runtimeAnimatorController = _session.Data.SwordIsActive ? _armed : _disArmed;
        }

        public void OnDoThrow()
        {
            if (_superThrow)
            {
                var numThrows = Mathf.Min(_superThrowParticles, ThrowingSwordCount);
                StartCoroutine(DoSuperThrow(numThrows));
            }

            else
            {
                ThrowAndRemoveOneItem();
            }

            _superThrow = false;
        }

        private IEnumerator DoSuperThrow(int numThrows)
        {
            for (int i = 0; i < numThrows; i++)
            {
                ThrowAndRemoveOneItem();
                yield return new WaitForSeconds(_superThrowDelay);
            }
        }

        private void ThrowAndRemoveOneItem()
        {
            Sounds.Play("Range");
            _particles.Spawn("Throw");
            _session.Data.Inventory.Remove("ThrowingSword", 1);
        }

        public void StartThrowing()
        {
            _superThrowCooldown.Reset();
        }

        public void PerformThrowing()
        {
            if (_session.Data.SwordIsActive)
            {
                if (!_throwCooldown.IsReady || ThrowingSwordCount <= 0) return;

                if (_superThrowCooldown.IsReady) _superThrow = true;

                Animator.SetTrigger(ThrowKey);
                _throwCooldown.Reset();
            }
        }

        public void ActivateSkill(string skill)
        {
            switch (skill)
            {
                case "Double Jump":
                    _session.Data.DoubleJumpIsActive = true;
                    break;

                case "Dash":
                    _session.Data.DashIsActive = true;
                    break;

                case "Wall Climb":
                    _session.Data.WallClimbIsActive = true;
                    break;

                case "Sword":
                    _session.Data.SwordIsActive = true;
                    Sounds.Play("Sword");
                    UpdateHeroWeapon();
                    break;
            }

            Debug.Log($"NEW SKILL: {skill}");
        }

        public void Use()
        {
            if (PotionsCount > 0)
            {
                Sounds.Play("Heal");
                _particles.Spawn("Heal");
                _health.ModifyHealth(5);
                _session.Data.Inventory.Remove("HealthPotion", 1);
            }
        }
    }
}