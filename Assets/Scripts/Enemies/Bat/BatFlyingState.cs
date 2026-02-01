using UnityEngine;

namespace Spelunky {

    /// <summary>
    /// Bat flies directly toward its target (the player).
    /// Ignores gravity and obstacles, flying in a straight line.
    /// </summary>
    public class BatFlyingState : EnemyState {

        public SpriteAnimation flyAnimation;

        public override void EnterState() {
            enemy.Visuals.animator.Play(flyAnimation);
        }

        public override void UpdateState() {
            if (enemy.target == null) {
                return;
            }

            Vector2 direction = (enemy.target.position - enemy.transform.position).normalized;
            enemy.velocity = direction * enemy.moveSpeed;
            enemy.FaceMovementDirection();
            enemy.Move();
        }

    }

}
