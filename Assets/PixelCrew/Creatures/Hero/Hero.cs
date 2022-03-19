using PixelCrew.Components;
using PixelCrew.Components.ColliderBased;
using PixelCrew.Components.GameObjectBased;
using PixelCrew.Components.Health;
using PixelCrew.Model;
using PixelCrew.Model.Data;
using PixelCrew.Model.Definitions;
using PixelCrew.Model.Definitions.Repository;
using PixelCrew.Model.Definitions.Repository.Item;
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
        [SerializeField] private LayerCheck _dashCheck;

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
        [SerializeField] private SpawnComponent _throwSpawner;

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

        private const string ThrowingSwordId = "ThrowingSword";
        private int CoinsCount => _session.Data.Inventory.Count("Coin");
        private int ThrowingSwordCount => _session.Data.Inventory.Count(ThrowingSwordId);
        private string SelectedItemId => _session.QuickInventory.SelectedItem.Id;

        private bool CanThrow
        {
            get
            {
                ////+++++++++++++++++++++++++++++++++++++++++++++++++++
                //if (SelectedItemId == ThrowingSwordId)
                //    return ThrowingSwordCount > 1;
                ////+++++++++++++++++++++++++++++++++++++++++++++++++++

                var definition = DefinitionsFacade.Instance.Items.Get(SelectedItemId);
                return definition.HasTag(ItemTag.Throwable);
            }
        }
        //private int PotionsCount => _session.Data.Inventory.Count("HealthPotion");

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

            _health.SetHealth(_session.Data.Hp.Value);
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
            if (id == ThrowingSwordId)
            {
                UpdateHeroWeapon();
            }
        }

        public void OnHealthChanged(int currentHealth)
        {
            _session.Data.Hp.Value = currentHealth;
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
            //var xVelocity = base.CalculateXVelocity();

            //if (_dash && _session.Data.DashIsActive)
            //{
            //    if (Direction.x != 0 && !_frontObjectsCheck.IsTouchingLayer)
            //    {
            //        _particles.Spawn("DashDust");
            //        _particles.Spawn("DashEffect");
            //        Sounds.Play("Dash");
            //    }

            //    Rigidbody.constraints = RigidbodyConstraints2D.FreezePositionY;
            //    Rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
            //    Rigidbody.constraints = _defaultConstraints;

            //    _dash = false;

            //    return xVelocity *= _dashForce;
            //}

            //return xVelocity;

            
            var xVelocity = base.CalculateXVelocity();

            if (_dash && _session.Data.DashIsActive)
            {
                if (Direction.x != 0 && !_frontObjectsCheck.IsTouchingLayer)
                {
                    _particles.Spawn("DashDust");
                    _particles.Spawn("DashEffect");
                    Sounds.Play("Dash");
                }

                Rigidbody.constraints = RigidbodyConstraints2D.FreezePositionY;
                Rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;

                if (!_dashCheck.IsTouchingLayer)
                {                    
                    _collider.enabled = false;
                    Invoke("EnableHeroCollider", 0.05f);
                }

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
                ////+++++++++++++++++++++++++++++++++++++++++++++++++++
                //var throwableCount = _session.Data.Inventory.Count(SelectedItemId);
                //var possibleCount = SelectedItemId == ThrowingSwordId ? throwableCount - 1 : throwableCount;
                ////+++++++++++++++++++++++++++++++++++++++++++++++++++

                var numThrows = Mathf.Min(_superThrowParticles, ThrowingSwordCount);//possibleCount
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

            var throwableId = _session.QuickInventory.SelectedItem.Id;
            var throwableDefinition = DefinitionsFacade.Instance.ThrowableItems.Get(throwableId);
            _throwSpawner.SetPrefab(throwableDefinition.Projectile);
            _throwSpawner.Spawn();

            //_particles.Spawn("Throw");
            _session.Data.Inventory.Remove(throwableId, 1);
        }

        public void StartThrowing()
        {
            _superThrowCooldown.Reset();
        }

        public void UseQuickItem()
        {
            if (IsSelectedItem(ItemTag.Throwable))
            {
                PerformThrowing();
            }

            else if (IsSelectedItem(ItemTag.Potion))
            {
                UsePotion();
            }
        }

        private void UsePotion()
        {
            var potion = DefinitionsFacade.Instance.Potions.Get(SelectedItemId);

            switch (potion.Effect)
            {
                case Effect.Heal:
                    _session.Data.Hp.Value += (int)potion.Value;
                    _particles.Spawn("Heal");
                    break;

                case Effect.SpeedUp:
                    _speedUpCooldown = new Cooldown { Value = potion.Time };
                    _additionalSpeed = potion.Value;
                    _speedUpCooldown.Reset();
                    _particles.Spawn("SpeedUp");
                    break;

                case Effect.Mana:
                    break;
            }

            _session.Data.Inventory.Remove(potion.Id, 1); 
        }

        private Cooldown _speedUpCooldown = new Cooldown();
        private float _additionalSpeed;

        protected override float CalculateSpeed()
        {
            if (_speedUpCooldown.IsReady)
            {
                _additionalSpeed = 0f;
            }

            return base.CalculateSpeed() + _additionalSpeed;
        }

        private bool IsSelectedItem(ItemTag tag)
        {
            return _session.QuickInventory.SelectedDef.HasTag(tag);
        }

        private void PerformThrowing()
        {
            if (_session.Data.SwordIsActive && !_frontObjectsCheck.IsTouchingLayer)
            {
                if (!_throwCooldown.IsReady || !CanThrow) return;

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

        public void SwitchItem()
        {
            _session.QuickInventory.SetNextItem();
        }
    }
}