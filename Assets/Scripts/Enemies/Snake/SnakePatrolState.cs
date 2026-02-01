using UnityEngine;

namespace Spelunky {

    /// <summary>
    /// Snake patrols back and forth, turning at walls and ledges.
    /// Attacks the player on contact with knockback.
    /// </summary>
    public class SnakePatrolState : EnemyState {

        [Header("Patrol Settings")]
        public bool turnAtLedges = true;
        public bool turnAtWalls = true;

        [Header("Animations")]
        public SpriteAnimation walkAnimation;
        public SpriteAnimation attackAnimation;

        [Header("Combat")]
        public float attackAnimationCooldown = 0.35f;
        private float _nextAttackAnimationTime;

        public override void EnterState() {
            enemy.Visuals.animator.Play(walkAnimation);
        }

        public override void UpdateState() {
            if (turnAtLedges && enemy.IsAtLedge()) {
                enemy.Visuals.FlipCharacter();
            }

            enemy.velocity.x = enemy.moveSpeed * enemy.Visuals.facingDirection;
            enemy.ApplyGravity();
            enemy.Move();
        }

        public override void OnEntityPhysicsCollisionEnter(CollisionInfo collisionInfo) {
            if (collisionInfo.left || collisionInfo.right) {
                if (turnAtWalls) {
                    enemy.Visuals.FlipCharacter();
                }
            }
        }

        public override void OnContactWithPlayer(Player player) {
            // If facing away from player, flip before attacking.
            float directionToPlayer = Mathf.Sign(player.transform.position.x - enemy.transform.position.x);
            if (directionToPlayer != 0 && directionToPlayer != enemy.Visuals.facingDirection) {
                enemy.Visuals.FlipCharacter();
            }

            if (Time.time < _nextAttackAnimationTime) {
                return;
            }

            enemy.Visuals.animator.PlayOnceUninterrupted(attackAnimation);
            _nextAttackAnimationTime = Time.time + attackAnimationCooldown;
        }

    }

}
