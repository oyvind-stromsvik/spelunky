using UnityEngine;

namespace Spelunky {

    /// <summary>
    /// Caveman runs back and forth, turning around when hitting walls.
    /// Chases the player by running horizontally.
    /// </summary>
    public class CavemanRunningState : EnemyState {

        [Header("Animation")]
        public string runAnimation = "Run";

        public override void EnterState() {
            if (!string.IsNullOrEmpty(runAnimation)) {
                enemy.Visuals.animator.Play(runAnimation);
            }
        }

        public override void UpdateState() {
            // Move horizontally in facing direction
            enemy.velocity.x = enemy.moveSpeed * enemy.Visuals.facingDirection;

            // Apply gravity
            enemy.ApplyGravity();

            // Move
            enemy.Move();
        }

        public override void OnCollisionEnter(CollisionInfo collisionInfo) {
            // Turn around when hitting walls
            if (collisionInfo.left || collisionInfo.right) {
                enemy.Visuals.FlipCharacter();
            }
        }

    }

}
