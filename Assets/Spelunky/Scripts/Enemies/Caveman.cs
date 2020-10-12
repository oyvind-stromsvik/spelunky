using UnityEngine;

namespace Spelunky {
    public class Caveman : Enemy
    {
        public float moveSpeed;
        public int damage;

        private Vector2 _velocity;

        private bool _triggered;

        private void Reset() {
            moveSpeed = 64f;
            damage = 1;
        }

        private void Update() {
            if (!_triggered) {
                return;
            }

            HandleCollisions();

            CalculateVelocity();

            EntityPhysics.Move(_velocity * Time.deltaTime);

            if (EntityPhysics.collisionInfo.down) {
                _velocity.y = 0;
            }
        }

        private void HandleCollisions() {
            if (EntityPhysics.collisionInfo.collidedThisFrame) {
                if (EntityPhysics.collisionInfo.right || EntityPhysics.collisionInfo.left) {
                    EntityVisuals.FlipCharacter();
                }
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
