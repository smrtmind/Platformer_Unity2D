using UnityEngine;

namespace PixelCrew.Creatures.Weapons
{
    public class Projectile : BaseProjectile
    {
        protected override void Start()
        {
            base.Start();

            var force = new Vector2(Direction * _speed, 0);
            Rigidbody.AddForce(force, ForceMode2D.Impulse);
        }
    }
}

//for throwing sword in kinematic rigidbody type
 
//using UnityEngine;

//namespace PixelCrew.Creatures.Weapons
//{
//    public class Projectile : MonoBehaviour
//    {
//        [SerializeField] private float _speed;

//        private Rigidbody2D _rigidbody;
//        private float _direction;

//        private void Start()
//        {
//            _direction = transform.lossyScale.x > 0 ? 1 : -1;
//            _rigidbody = GetComponent<Rigidbody2D>();
//        }

//        private void FixedUpdate()
//        {
//            var position = _rigidbody.position;
//            position.x += _direction * _speed;
//            _rigidbody.MovePosition(position);
//        }
//    }
//}
