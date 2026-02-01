using UnityEngine;

namespace Spelunky {

    /// <summary>
    /// Bat waits idle until the player enters its trigger zone.
    /// Once triggered, transitions to flying state to chase the player.
    /// </summary>
    public class BatIdleState : EnemyState {

        public SpriteAnimation idleAnimation;

        [Tooltip("State to enter when player is detected")]
        public EnemyState activatedState;
        
        public override void EnterState() {
            enemy.velocity = Vector2.zero;
            enemy.Visuals.animator.Play(idleAnimation);
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
            enemy.stateMachine.AttemptToChangeState(activatedState);
        }

    }

}
