using PixelCrew.Components;
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

        [SerializeField] private float _groundCheckRadius;
        [SerializeField] private Vector3 _groundCheckPositionDelta;

        [SerializeField] private AnimatorController _armed;
        [SerializeField] private AnimatorController _disArmed;

        [SerializeField] private CheckCircleOverlap _attackRange;

        [Space] [Header("Particles")]
        [SerializeField] private SpawnComponent _runDustParticles;
        [SerializeField] private SpawnComponent _jumpDustParticles;
        [SerializeField] private SpawnComponent _fallDustParticles;
        [SerializeField] private ParticleSystem _hitParticles;
        [SerializeField] private SpawnComponent _dashWaveParticles;

        private readonly Collider2D[] _interactionResult = new Collider2D[1];
        private Rigidbody2D _rigidbody;
        private Vector2 _direction;
        private Animator _animator;
        private bool _isGrounded;
        private bool _allowDoubleJump;
        private bool _isJumping;

        //skills
        private bool _doubleJumpIsActive;
        private bool _dashIsActive;
        private bool _isArmed;

        private float _jump;
        private int _coins;
        private bool _dash;

        private static readonly int IsGroundKey = Animator.StringToHash("is-ground");
        private static readonly int IsRunningKey = Animator.StringToHash("is-running");
        private static readonly int VerticalVelocityKey = Animator.StringToHash("vertical-velocity");
        private static readonly int Hit = Animator.StringToHash("is-hit");
        private static readonly int AttackKey = Animator.StringToHash("attack");

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
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
        }

        public void Dash()
        {
            _dash = true;
        }

        public void Attack()
        {
            if (!_isArmed) return;

            _animator.SetTrigger(AttackKey);
        }

        public void OnAttack()
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

            if (_dash && _dashIsActive)
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
                if (_doubleJumpIsActive)
                {
                    _allowDoubleJump = true;
                }

                _isJumping = false;
            }

            if (isJumpPressing)
            {
                _isJumping = true;
                yVelocity = CalculateJumpVelocity(yVelocity);
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
            else if (_allowDoubleJump && _doubleJumpIsActive)
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
            _coins += coins;
            Debug.Log($"+{coins} coins. TOTAL COINS: {_coins}");
        }

        public void TakeDamage()
        {
            _isJumping = false;
            _animator.SetTrigger(Hit);
            _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, _damageJumpForce);

            if (_coins > 0)
            {
                SpawnCoins();
            }
        }

        private void SpawnCoins()
        {
            var numCoinsToDispose = Mathf.Min(_coins, 5);
            _coins -= numCoinsToDispose;

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
            }
        }

        public void SpawnFootDust()
        {
            _runDustParticles.Spawn();
        }

        public void SetCoinsToDefault()
        {
            _coins = default;
        }

        public void ActivateDoubleJump()
        {
            _doubleJumpIsActive = true;
            Debug.Log("NEW SKILL: DOUBLE JUMP");
        }

        public void ActivateAirDash()
        {
            _dashIsActive = true;
            Debug.Log("NEW SKILL: DASH");
        }

        public void ArmHero()
        {
            _isArmed = true;
            _animator.runtimeAnimatorController = _armed;
        }
    }
}