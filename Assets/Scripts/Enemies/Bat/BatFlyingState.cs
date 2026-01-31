using UnityEngine;

namespace Spelunky {

    /// <summary>
    /// Bat flies directly toward its target (the player).
    /// Ignores gravity and obstacles, flying in a straight line.
    /// </summary>
    public class BatFlyingState : EnemyState {

        [Header("Animation")]
        public string flyAnimation = "Fly";

        public override void EnterState() {
            if (!string.IsNullOrEmpty(flyAnimation)) {
                enemy.Visuals.animator.Play(flyAnimation);
            }
        }

        public override void UpdateState() {
            if (enemy.target == null) {
                return;
            }

            // Calculate direction to target and move directly toward it
            Vector2 direction = (enemy.target.position - enemy.transform.position).normalized;
            enemy.velocity = direction * enemy.moveSpeed;

            // Face the direction we're moving
            enemy.FaceMovementDirection();

            // Move the bat
            enemy.Move();
        }

    }

}
