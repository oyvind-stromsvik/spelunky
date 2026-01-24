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
        public float attackAnimationCooldown = 0.35f;
        private float _nextAttackAnimationTime;

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
                if (turnAtWalls) {
                    // Hit a wall - turn around
                    enemy.Visuals.FlipCharacter();
                }
            }
        }

        public override void OnContactWithPlayer(Player player) {
            // If facing away from player, flip.
            float directionToPlayer = Mathf.Sign(player.transform.position.x - enemy.transform.position.x);
            if (directionToPlayer != 0 && directionToPlayer != enemy.Visuals.facingDirection) {
                enemy.Visuals.FlipCharacter();
            }
            
            if (string.IsNullOrEmpty(attackAnimation)) {
                return;
            }

            if (Time.time < _nextAttackAnimationTime) {
                return;
            }

            enemy.Visuals.animator.PlayOnceUninterrupted(attackAnimation);
            _nextAttackAnimationTime = Time.time + attackAnimationCooldown;
        }

    }

}
