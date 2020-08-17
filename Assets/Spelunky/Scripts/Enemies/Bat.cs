using System;
using UnityEngine;

namespace Spelunky {
    public class Bat : Enemy
    {
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
            PhysicsObject.Move(_velocity * Time.deltaTime);
        }

        private void CalculateVelocity() {
            _velocity = (_targetToMoveTowards.position - transform.position).normalized * moveSpeed;
        }

        public override bool IgnoreCollider(Collider2D collider, CollisionDirection direction) {
            if (collider.CompareTag("Player")) {
                collider.GetComponent<Player>().TakeDamage(damage, direction);
                return true;
            }

            return false;
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag("Player")) {
                _targetToMoveTowards = other.transform;
                EntityVisuals.animator.Play("Fly");
            }
        }
    }
}
