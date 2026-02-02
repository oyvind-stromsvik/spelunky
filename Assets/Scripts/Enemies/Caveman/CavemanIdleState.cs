using UnityEngine;

namespace Spelunky {

    /// <summary>
    /// Caveman stands idle until the player enters its trigger zone.
    /// Once triggered, transitions to running state to chase the player.
    /// </summary>
    public class CavemanIdleState : EnemyState {

        public SpriteAnimation idleAnimation;

        [Tooltip("State to enter when player is detected")]
        public EnemyState activatedState;

        public override void EnterState() {
            enemy.velocity = Vector2.zero;
            enemy.Visuals.animator.Play(idleAnimation);
        }

        public override void UpdateState() {
            enemy.ApplyGravity();
            enemy.Move();
            
            Transform detected = DetectPlayer();
            if (detected != null) {
                Activate(detected);
            }
        }

        public override void OnEnemyTriggerEnter(Collider2D other) {
            if (other.CompareTag("Player")) {
                Activate(other.transform);
            }
        }

        private Transform DetectPlayer() {
            Vector2 origin = (Vector2)enemy.transform.position + new Vector2(0, enemy.Physics.Collider.size.y);
            Vector2 direction = Vector2.left;
            RaycastHit2D hit = Physics2D.Raycast(origin, direction, enemy.detectionRange, enemy.targetDetectionMask);
            Debug.DrawRay(origin, direction * enemy.detectionRange, Color.green);
            return hit.collider != null ? hit.transform : null;
        }

        private void Activate(Transform target) {
            enemy.target = target;
            enemy.isActivated = true;
            if (activatedState != null) {
                enemy.stateMachine.AttemptToChangeState(activatedState);
            }
        }

    }

}
