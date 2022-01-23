using PixelCrew.Components;
using PixelCrew.Model;
using PixelCrew.Utils;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace PixelCrew
{
    public class Hero : MonoBehaviour
    {
        [SerializeField] private float _speed;
        [SerializeField] private float _jumpForce;
        [SerializeField] private float _damageJumpForce;
        [SerializeField] private float _slamDownVelocity;
        [SerializeField] private float _dashForce;
        [SerializeField] private int _damage;
        //[SerializeField] private Transform _airDash;
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private float _interactionRadius;
        [SerializeField] private LayerMask _interactionLayer;
        [SerializeField] private float _damageVelocity;
        [SerializeField] private LayerCheck _wallCheck;

        [SerializeField] private float _groundCheckRadius;
        [SerializeField] private Vector3 _groundCheckPositionDelta;

        [SerializeField] private AnimatorController _armed;
        [SerializeField] private AnimatorController _disArmed;

        [SerializeField] private CheckCircleOverlap _attackRange;

        [Space]
        [Header("Particles")]
        [SerializeField] private SpawnComponent _runDustParticles;
        [SerializeField] private SpawnComponent _jumpDustParticles;
        [SerializeField] private SpawnComponent _fallDustParticles;
        [SerializeField] private ParticleSystem _hitParticles;
        [SerializeField] private SpawnComponent _dashWaveParticles;
        [SerializeField] private SpawnComponent _swordSlashParticles;

        private readonly Collider2D[] _interactionResult = new Collider2D[1];
        private Rigidbody2D _rigidbody;
        private Vector2 _direction;
        private Animator _animator;
        private bool _isGrounded;
        private bool _allowDoubleJump;
        private bool _isJumping;
        private bool _isOnWall;

        private float _jump;
        private bool _dash;

        private static readonly int IsGroundKey = Animator.StringToHash("is-ground");
        private static readonly int IsRunningKey = Animator.StringToHash("is-running");
        private static readonly int VerticalVelocityKey = Animator.StringToHash("vertical-velocity");
        private static readonly int HitKey = Animator.StringToHash("is-hit");
        private static readonly int AttackKey = Animator.StringToHash("attack");
        private static readonly int OnTheWallKey = Animator.StringToHash("is-onTheWall");

        private GameSession _session;
        private float _defaultGravityScale;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
            _defaultGravityScale = _rigidbody.gravityScale;
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

        public void SetDirection(Vector2 direction)
        {
            _direction = direction;
        }

        public void SetJump(float jump)
        {
            _jump = jump;
        }

        private void Update()
        {
            _isGrounded = IsGrounded();

            if (_session.Data.OnWallHookIsActive)
            {
                if (_wallCheck.IsTouchingLayer && _direction.x == transform.localScale.x)
                {
                    _isOnWall = true;
                    _rigidbody.gravityScale = 0;

                    _animator.SetBool(OnTheWallKey, true);
                }

                else
                {
                    _animator.SetBool(OnTheWallKey, false);

                    _isOnWall = false;
                    _rigidbody.gravityScale = _defaultGravityScale;
                }
            }
        }

        public void Dash()
        {
            _dash = true;
        }

        public void Attack()
        {
            if (!_session.Data.IsArmed) return;

            _animator.SetTrigger(AttackKey);
            _swordSlashParticles.Spawn();
        }

        public void OnDoAttack()
        {
            var gos = _attackRange.GetObjectsInRange();
            foreach (var go in gos)
            {
                var hp = go.GetComponent<HealthComponent>();
                if (hp != null && go.CompareTag("Enemy"))
                {
                    hp.ModifyHealth(-_damage);
                }
            }
        }

        private void FixedUpdate()
        {
            var xVelocity = CalculateXVelocity();
            var yVelocity = CalculateYVelocity();
            _rigidbody.velocity = new Vector2(xVelocity, yVelocity);

            _animator.SetBool(IsGroundKey, _isGrounded);
            _animator.SetBool(IsRunningKey, _direction.x != 0);
            _animator.SetFloat(VerticalVelocityKey, _rigidbody.velocity.y);

            UpdateSpriteDirection();
        }

        private float CalculateXVelocity()
        {
            var xVelocity = _direction.x * _speed;

            if (_dash && _session.Data.DashIsActive)
            {
                if (_direction.x != 0)
                {
                    _dashWaveParticles.Spawn();
                }

                _rigidbody.constraints = RigidbodyConstraints2D.FreezePositionY;
                _rigidbody.constraints = RigidbodyConstraints2D.None;
                _rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;

                _dash = false;

                return xVelocity *= _dashForce;
            }

            return xVelocity;
        }

        private float CalculateYVelocity()
        {
            var yVelocity = _rigidbody.velocity.y;
            var isJumpPressing = _jump > 0;//_direction.y > 0

            //check if you push up
            //if (_direction.y > 0)
            //{
            //    Debug.Log("PUSHED UP");
            //}

            if (_isGrounded)
            {
                if (_session.Data.DoubleJumpIsActive)
                {
                    _allowDoubleJump = true;
                }

                _isJumping = false;
            }

            if (_isOnWall)
            {
                _allowDoubleJump = true;
            }

            if (isJumpPressing)
            {
                _isJumping = true;
                yVelocity = CalculateJumpVelocity(yVelocity);
            }

            else if (_isOnWall)
            {
                yVelocity = 0f;
            }

            else if (_rigidbody.velocity.y > 0 && _isJumping)
            {
                yVelocity *= 0.5f;
            }

            return yVelocity;
        }

        private float CalculateJumpVelocity(float yVelocity)
        {
            var isFalling = _rigidbody.velocity.y <= 0.001f;
            if (!isFalling) return yVelocity;

            if (_isGrounded)
            {
                yVelocity = _jumpForce;
                _jumpDustParticles.Spawn();
            }
            else if (_allowDoubleJump && _session.Data.DoubleJumpIsActive)
            {
                yVelocity = _jumpForce;
                _jumpDustParticles.Spawn();
                _allowDoubleJump = false;
            }

            return yVelocity;
        }

        private void UpdateSpriteDirection()
        {
            if (_direction.x > 0)
            {
                //the same as transform.localScale = new Vector3(1, 1, 1);
                transform.localScale = Vector3.one;
            }

            else if (_direction.x < 0)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
        }

        private bool IsGrounded()
        {
            var hit = Physics2D.CircleCast(transform.position + _groundCheckPositionDelta, _groundCheckRadius,
                Vector2.down, 0, _groundLayer);
            return hit.collider != null;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Handles.color = IsGrounded() ? HandlesUtils.TransparentGreen : HandlesUtils.TransparentRed;
            Handles.DrawSolidDisc(transform.position + _groundCheckPositionDelta, Vector3.forward, _groundCheckRadius);
        }
#endif

        public void SaySomething()
        {
            Debug.Log("Hello!");
        }

        public void AddCoins(int coins)
        {
            _session.Data.Coins += coins;
            Debug.Log($"+{coins} coins. TOTAL COINS: {_session.Data.Coins}");
        }

        public void TakeDamage()
        {
            _isJumping = false;
            _animator.SetTrigger(HitKey);
            _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, _damageJumpForce);

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
            var size = Physics2D.OverlapCircleNonAlloc(
                transform.position,
                _interactionRadius,
                _interactionResult,
                _interactionLayer);

            for (int i = 0; i < size; i++)
            {
                var interactable = _interactionResult[i].GetComponent<InteractableComponent>();
                if (interactable != null)
                {
                    interactable.Interact();
                }
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.IsInLayer(_groundLayer))
            {
                var contact = other.contacts[0];
                if (contact.relativeVelocity.y >= _slamDownVelocity)
                {
                    _fallDustParticles.Spawn();
                }

                if (contact.relativeVelocity.y >= _damageVelocity)
                {
                    GetComponent<HealthComponent>().ModifyHealth(-1);
                }
            }
        }

        public void SpawnFootDust()
        {
            _runDustParticles.Spawn();
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

        public void ActivateOnWallHook()
        {
            _session.Data.OnWallHookIsActive = true;
            Debug.Log("NEW SKILL: HOOK (stick to walls)");
        }

        public void ArmHero()
        {
            _session.Data.IsArmed = true;
            UpdateHeroWeapon();
        }

        private void UpdateHeroWeapon()
        {
            //ternary
            _animator.runtimeAnimatorController = _session.Data.IsArmed ? _armed : _disArmed;
        }

        public void Throw()
        {

        }
    }
}