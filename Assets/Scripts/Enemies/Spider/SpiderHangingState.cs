using UnityEngine;

namespace Spelunky {

    /// <summary>
    /// Spider hangs from ceiling until it detects the player below.
    /// Falls when triggered and transitions to jumping state on landing.
    /// </summary>
    public class SpiderHangingState : EnemyState {

        [Header("Detection")]
        [Tooltip("Layer mask for the ceiling/block to hang from")]
        public LayerMask ceilingMask;

        [Header("State Transitions")]
        [Tooltip("State to enter after landing")]
        public EnemyState landedState;

        [Header("Animation")]
        public SpriteAnimation hangingAnimation;
        public SpriteAnimation fallingAnimation;

        private Collider2D _ceilingCollider;
        private bool _isFalling;

        public override void EnterState() {
            _isFalling = false;

            // Find ceiling above us
            RaycastHit2D hit = Physics2D.Raycast(enemy.transform.position, Vector2.up, 24f, ceilingMask);
            _ceilingCollider = hit.collider;
            
            enemy.Visuals.animator.Play(hangingAnimation);
        }

        public override void UpdateState() {
            // Check if our ceiling was destroyed
            if (_ceilingCollider == null && !_isFalling) {
                StartFalling();
                // When ceiling is destroyed, auto-target player globally.
                enemy.target = DetectPlayerGlobally();
            }

            // Detect player below if still hanging
            if (!_isFalling) {
                Transform detected = DetectPlayerBelow();
                if (detected != null) {
                    enemy.target = detected;
                    StartFalling();
                }

                return;
            }

            // Falling - apply gravity and move
            enemy.velocity.y += PhysicsManager.gravity.y * Time.deltaTime;
            enemy.Move();
        }

        private void StartFalling() {
            _isFalling = true;
            enemy.Visuals.animator.Play(fallingAnimation);
        }

        /// <summary>
        /// Detect player directly below the spider using a raycast.
        /// </summary>
        private Transform DetectPlayerBelow() {
            RaycastHit2D hit = Physics2D.Raycast(enemy.transform.position, Vector2.down, enemy.detectionRange, enemy.targetDetectionMask);
            Debug.DrawRay(enemy.transform.position, Vector2.down * enemy.detectionRange, Color.green);
            return hit.collider != null ? hit.transform : null;
        }

        /// <summary>
        /// Detect player anywhere in the level (used when ceiling is destroyed).
        /// Matches original Spelunky behavior where spiders auto-target the player
        /// if their hanging block is destroyed.
        /// </summary>
        private Transform DetectPlayerGlobally() {
            float radius = 1000f; // Large radius to find player anywhere
            Collider2D hit = Physics2D.OverlapCircle(enemy.transform.position, radius, enemy.targetDetectionMask);
            return hit != null ? hit.transform : null;
        }

        public override void OnEntityPhysicsCollisionEnter(CollisionInfo collisionInfo) {
            if (_isFalling && collisionInfo.becameGroundedThisFrame) {
                if (landedState != null) {
                    enemy.stateMachine.AttemptToChangeState(landedState);
                }
            }
        }

    }

}
