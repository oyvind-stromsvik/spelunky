using UnityEngine;

namespace Spelunky {

    public class Snake : Entity {

        public float moveSpeed;
        public int damage;

        public Vector2 _velocity;

        private void Reset() {
            moveSpeed = 16f;
            damage = 1;
        }

        private void Update() {
            HandleUnsteady();

            if (Physics.collisionInfo.right || Physics.collisionInfo.left) {
                Visuals.FlipCharacter();
            }

            _velocity.x = moveSpeed * Visuals.facingDirection;

            if (Physics.collisionInfo.down) {
                _velocity.y = 0;
            }
            else {
                _velocity.y += PhysicsManager.gravity.y * Time.deltaTime;
            }

            Physics.Move(_velocity * Time.deltaTime);
        }

        private void HandleUnsteady() {
            Vector3 offsetForward = new Vector3(Physics.Collider.size.x * Visuals.facingDirection / 2f, 1, 0);
            RaycastHit2D hitForward = Physics2D.Raycast(transform.position + offsetForward, Vector2.down, 2, Physics.collisionMask);
            Debug.DrawRay(transform.position + offsetForward, Vector2.down * 2, Color.green);

            // Play unsteady animation
            if (Physics.collisionInfo.down && hitForward.collider == null) {
                Visuals.FlipCharacter();
            }
        }
    }

}
