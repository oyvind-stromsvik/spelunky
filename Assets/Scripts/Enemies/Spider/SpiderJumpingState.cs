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
        public SpriteAnimation idleAnimation;
        public SpriteAnimation jumpAnimation;
        public SpriteAnimation fallAnimation;

        private float _idleTimer;
        private bool _isJumping;

        public override void EnterState() {
            _idleTimer = Random.Range(minJumpWaitTime, maxJumpWaitTime);
            _isJumping = false;
            enemy.velocity = Vector2.zero;
            enemy.Visuals.animator.Play(idleAnimation);
        }

        public override void UpdateState() {
            enemy.velocity.y += PhysicsManager.gravity.y * Time.deltaTime;
            enemy.Move();

            if (enemy.Physics.collisionInfo.down) {
                _isJumping = false;
                enemy.velocity = Vector2.zero;

                _idleTimer -= Time.deltaTime;
                if (_idleTimer <= 0f) {
                    Jump();
                }
                else {
                    enemy.Visuals.animator.Play(idleAnimation);
                }
            }
            else {
                if (_isJumping) {
                    if (enemy.velocity.y > 0) {
                        enemy.Visuals.animator.Play(jumpAnimation, 1, false);
                    }
                    else {
                        enemy.Visuals.animator.Play(fallAnimation, 1, false);
                    }
                }
            }
        }

        private void Jump() {
            if (enemy.target == null) {
                return;
            }

            float direction = Mathf.Sign(enemy.target.position.x - enemy.transform.position.x);
            enemy.velocity = new Vector2(jumpVelocity.x * direction, jumpVelocity.y);
            _isJumping = true;
        }

        public override void OnEntityPhysicsCollisionEnter(CollisionInfo collisionInfo) {
            if (collisionInfo.up) {
                enemy.velocity.y = 0;
            }

            if (collisionInfo.left || collisionInfo.right) {
                enemy.velocity.x *= -0.25f;
            }

            if (collisionInfo.becameGroundedThisFrame) {
                _idleTimer = Random.Range(minJumpWaitTime, maxJumpWaitTime);
            }
        }

    }

}
