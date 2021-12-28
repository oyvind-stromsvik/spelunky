using UnityEngine;

namespace Spelunky {

    public class Bat : Entity {

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

            if (_velocity.x > 0 && !Visuals.isFacingRight) {
                Visuals.FlipCharacter();
            }
            else if (_velocity.x < 0 && Visuals.isFacingRight) {
                Visuals.FlipCharacter();
            }

            _velocity = (_targetToMoveTowards.position - transform.position).normalized * moveSpeed;

            Physics.Move(_velocity * Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag("Player")) {
                _targetToMoveTowards = other.transform;
                Visuals.animator.Play("Fly");
            }
        }
    }

}
