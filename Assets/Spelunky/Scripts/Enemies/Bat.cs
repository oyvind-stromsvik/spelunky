using UnityEngine;

namespace Spelunky {

    public class Bat : Enemy {
        public float moveSpeed;
        public int damage;

        private Vector2 _velocity;

        private Transform _targetToMoveTowards;

        private void Reset() {
            moveSpeed = 24f;
            damage = 1;
        }

        private void Update() {
            if (_targetToMoveTowards == null) {
                return;
            }

            if (_velocity.x > 0 && !EntityVisuals.isFacingRight) {
                EntityVisuals.FlipCharacter();
            }
            else if (_velocity.x < 0 && EntityVisuals.isFacingRight) {
                EntityVisuals.FlipCharacter();
            }

            CalculateVelocity();
            EntityPhysics.Move(_velocity * Time.deltaTime);
        }

        private void CalculateVelocity() {
            _velocity = (_targetToMoveTowards.position - transform.position).normalized * moveSpeed;
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag("Player")) {
                _targetToMoveTowards = other.transform;
                EntityVisuals.animator.Play("Fly");
            }
        }
    }

}