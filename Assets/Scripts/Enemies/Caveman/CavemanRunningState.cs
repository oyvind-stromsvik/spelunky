using UnityEngine;

namespace Spelunky {

    /// <summary>
    /// Caveman runs back and forth, turning around when hitting walls.
    /// Chases the player by running horizontally.
    /// </summary>
    public class CavemanRunningState : EnemyState {

        [Header("Animation")]
        public SpriteAnimation runAnimation;

        public override void EnterState() {
            enemy.Visuals.animator.Play(runAnimation);
        }

        public override void UpdateState() {
            enemy.velocity.x = enemy.moveSpeed * enemy.Visuals.facingDirection;
            enemy.ApplyGravity();
            enemy.Move();
        }

        public override void OnEntityPhysicsCollisionEnter(CollisionInfo collisionInfo) {
            if (collisionInfo.left || collisionInfo.right) {
                enemy.Visuals.FlipCharacter();
            }
        }

    }

}
