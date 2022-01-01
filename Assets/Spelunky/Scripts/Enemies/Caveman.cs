using UnityEngine;

namespace Spelunky {

    public class Caveman : Entity {

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

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag("Player")) {
                _triggered = true;
                Visuals.animator.Play("Run");
            }
        }
    }

}
