using UnityEngine;

namespace Spelunky {

    /// <summary>
    /// Bat waits idle until the player enters its trigger zone.
    /// Once triggered, transitions to flying state to chase the player.
    /// </summary>
    public class BatIdleState : EnemyState {

        [Header("State Transitions")]
        [Tooltip("State to enter when player is detected")]
        public EnemyState activatedState;

        [Header("Animation")]
        [Tooltip("Animation to play while idle (optional)")]
        public string idleAnimation = "";

        [Tooltip("Animation to play when activated")]
        public string activationAnimation = "Fly";

        public override void EnterState() {
            enemy.velocity = Vector2.zero;

            if (!string.IsNullOrEmpty(idleAnimation)) {
                enemy.Visuals.animator.Play(idleAnimation);
            }
        }

        public override void UpdateState() {
            Transform target = enemy.DetectTargetInBox((Vector2)transform.position + enemy.detectionOffset, enemy.detectionBox, 0f);
            if (target != null) {
                Activate(target);
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
