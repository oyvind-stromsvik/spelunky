using UnityEngine;

namespace Spelunky {

    /// <summary>
    /// Spider jumps toward its target periodically after landing from hanging.
    /// Waits a random amount of time between jumps while grounded.
    /// </summary>
    public class SpiderJumpingState : EnemyState {

        [Header("Jump Settings")]
        public Vector2 jumpVelocity = new Vector2(96, 196);
        public float minJumpWaitTime = 1f;
        public float maxJumpWaitTime = 3f;

        [Header("Animation")]
        public string idleAnimation = "Idle";
        public string jumpAnimation = "Jump";
        public string fallAnimation = "Fall";

        private float _idleTimer;
        private bool _isJumping;

        public override void EnterState() {
            _idleTimer = Random.Range(minJumpWaitTime, maxJumpWaitTime);
            _isJumping = false;
            enemy.velocity = Vector2.zero;

            if (!string.IsNullOrEmpty(idleAnimation)) {
                enemy.Visuals.animator.Play(idleAnimation);
            }
        }

        public override void UpdateState() {
            // Apply gravity
            enemy.velocity.y += PhysicsManager.gravity.y * Time.deltaTime;
            enemy.Move();

            if (enemy.Physics.collisionInfo.down) {
                // Grounded - wait then jump
                _isJumping = false;
                enemy.velocity = Vector2.zero;

                _idleTimer -= Time.deltaTime;
                if (_idleTimer <= 0f) {
                    Jump();
                }
                else if (!string.IsNullOrEmpty(idleAnimation)) {
                    enemy.Visuals.animator.Play(idleAnimation);
                }
            }
            else {
                // In air - update animation based on velocity
                if (_isJumping) {
                    if (enemy.velocity.y > 0) {
                        if (!string.IsNullOrEmpty(jumpAnimation)) {
                            enemy.Visuals.animator.Play(jumpAnimation, 1, false);
                        }
                    }
                    else {
                        if (!string.IsNullOrEmpty(fallAnimation)) {
                            enemy.Visuals.animator.Play(fallAnimation, 1, false);
                        }
                    }
                }
            }
        }

        private void Jump() {
            if (enemy.target == null) {
                return;
            }

            // Jump toward the target
            float direction = Mathf.Sign(enemy.target.position.x - enemy.transform.position.x);
            enemy.velocity = new Vector2(jumpVelocity.x * direction, jumpVelocity.y);
            _isJumping = true;
        }

        public override void OnCollisionEnter(CollisionInfo collisionInfo) {
            // Hit ceiling - stop upward velocity
            if (collisionInfo.up) {
                enemy.velocity.y = 0;
            }

            // Hit wall - reduce horizontal velocity
            if (collisionInfo.left || collisionInfo.right) {
                enemy.velocity.x *= -0.25f;
            }

            // Just landed - reset jump timer
            if (collisionInfo.becameGroundedThisFrame) {
                _idleTimer = Random.Range(minJumpWaitTime, maxJumpWaitTime);
            }
        }

    }

}
