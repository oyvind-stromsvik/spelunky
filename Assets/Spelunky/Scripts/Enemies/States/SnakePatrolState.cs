using UnityEngine;

namespace Spelunky {

    /// <summary>
    /// Snake patrols back and forth, turning at walls and ledges.
    /// Attacks the player on contact with knockback.
    /// </summary>
    public class SnakePatrolState : EnemyState {

        [Header("Patrol Settings")]
        [Tooltip("Turn around at ledges")]
        public bool turnAtLedges = true;

        [Tooltip("Turn around when hitting walls")]
        public bool turnAtWalls = true;

        [Header("Animation")]
        public string walkAnimation = "";
        public string attackAnimation = "Attack";

        [Header("Combat")]
        [Tooltip("Knockback applied to player on hit")]
        public Vector2 knockback = new Vector2(256, 512);

        public override void EnterState() {
            if (!string.IsNullOrEmpty(walkAnimation)) {
                enemy.Visuals.animator.Play(walkAnimation);
            }
        }

        public override void UpdateState() {
            // Check for ledges
            if (turnAtLedges && enemy.IsAtLedge()) {
                enemy.Visuals.FlipCharacter();
            }

            // Move in facing direction
            enemy.velocity.x = enemy.moveSpeed * enemy.Visuals.facingDirection;

            // Apply gravity
            enemy.ApplyGravity();

            // Move
            enemy.Move();
        }

        public override void OnCollisionEnter(CollisionInfo collisionInfo) {
            if (collisionInfo.left || collisionInfo.right) {
                // Check if we hit the player
                if (collisionInfo.colliderHorizontal.CompareTag("Player")) {
                    AttackPlayer(collisionInfo.colliderHorizontal);
                }
                else if (turnAtWalls) {
                    // Hit a wall - turn around
                    enemy.Visuals.FlipCharacter();
                }
            }
        }

        private void AttackPlayer(Collider2D playerCollider) {
            Player player = playerCollider.GetComponent<Player>();
            if (player == null) {
                return;
            }

            // Play attack animation
            if (!string.IsNullOrEmpty(attackAnimation)) {
                enemy.Visuals.animator.PlayOnceUninterrupted(attackAnimation);
            }

            // Apply knockback in the direction the snake is facing
            Vector2 appliedKnockback = new Vector2(knockback.x * enemy.Visuals.facingDirection, knockback.y);
            enemy.DealDamage(player, appliedKnockback);
        }

    }

}
