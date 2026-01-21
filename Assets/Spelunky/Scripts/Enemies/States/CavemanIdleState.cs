using UnityEngine;

namespace Spelunky {

    /// <summary>
    /// Caveman stands idle until the player enters its trigger zone.
    /// Once triggered, transitions to running state to chase the player.
    /// </summary>
    public class CavemanIdleState : EnemyState {

        [Header("State Transitions")]
        [Tooltip("State to enter when player is detected")]
        public EnemyState activatedState;

        [Header("Animation")]
        [Tooltip("Animation to play while idle (optional)")]
        public string idleAnimation = "";

        [Tooltip("Animation to play when activated")]
        public string activationAnimation = "Run";

        public override void EnterState() {
            enemy.velocity = Vector2.zero;

            if (!string.IsNullOrEmpty(idleAnimation)) {
                enemy.Visuals.animator.Play(idleAnimation);
            }
        }

        public override void UpdateState() {
            // Apply gravity so caveman stays grounded
            enemy.ApplyGravity();
            enemy.Move();
        }

        public override void OnTriggerEnter(Collider2D other) {
            if (other.CompareTag("Player")) {
                Activate(other.transform);
            }
        }

        private void Activate(Transform target) {
            enemy.target = target;
            enemy.isActivated = true;

            if (!string.IsNullOrEmpty(activationAnimation)) {
                enemy.Visuals.animator.Play(activationAnimation);
            }

            if (activatedState != null) {
                enemy.stateMachine.AttemptToChangeState(activatedState);
            }
        }

    }

}
