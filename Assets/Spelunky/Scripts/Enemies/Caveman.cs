using UnityEngine;

namespace Spelunky {

    public class Caveman : Enemy {
        public float moveSpeed;
        public int damage;

        private Vector2 _velocity;

        private bool _triggered;

        private void Reset() {
            moveSpeed = 64f;
            damage = 1;
        }

        public override void Awake() {
            base.Awake();
            EntityPhysics.OnCollisionEvent.AddListener(OnCollision);
        }

        private void Update() {
            if (!_triggered) {
                return;
            }

            CalculateVelocity();

            EntityPhysics.Move(_velocity * Time.deltaTime);

            if (EntityPhysics.collisionInfo.down) {
                _velocity.y = 0;
            }
        }

        public void OnCollision(CollisionInfo collisionInfo) {
            if (collisionInfo.right || collisionInfo.left) {
                EntityVisuals.FlipCharacter();
            }
        }

        private void CalculateVelocity() {
            _velocity.x = moveSpeed * EntityVisuals.facingDirection;
            _velocity.y += PhysicsManager.gravity.y * Time.deltaTime;
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag("Player")) {
                _triggered = true;
                EntityVisuals.animator.Play("Run");
            }
        }
    }

}
