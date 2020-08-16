using UnityEngine;

namespace Spelunky {
    public class Snake : Enemy {

        public float moveSpeed;
        public int damage;

        private Vector2 _velocity;

        private void Reset() {
            moveSpeed = 16f;
            damage = 1;
        }

        private void Update() {
            HandleUnsteady();

            HandleCollisions();

            CalculateVelocity();

            PhysicsObject.Move(_velocity * Time.deltaTime);

            if (PhysicsObject.collisions.below) {
                _velocity.y = 0;
            }
        }

        private void HandleCollisions() {
            if (PhysicsObject.collisions.collidedThisFrame) {
                if (PhysicsObject.collisions.right || PhysicsObject.collisions.left) {
                    EntityVisuals.FlipCharacter();
                }
            }
        }

        private void CalculateVelocity() {
            _velocity.x = moveSpeed * EntityVisuals.facingDirection;
            _velocity.y += PhysicsManager.gravity.y * Time.deltaTime;
        }

        private void HandleUnsteady() {
            Vector3 offsetForward = new Vector3(PhysicsObject.Collider.size.x * EntityVisuals.facingDirection / 2f, 1, 0);
            RaycastHit2D hitForward = Physics2D.Raycast(transform.position + offsetForward, Vector2.down, 2, PhysicsObject.collisionMask);
            Debug.DrawRay(transform.position + offsetForward, Vector2.down * 2, Color.green);

            // Play unsteady animation
            if (PhysicsObject.collisions.below && hitForward.collider == null) {
                EntityVisuals.FlipCharacter();
            }
        }

        public override bool IgnoreCollision(Collider2D collider, CollisionDirection direction) {
            if (collider.CompareTag("Player")) {
                collider.GetComponent<Player>().TakeDamage(damage, direction);
                return true;
            }

            return false;
        }
    }
}
