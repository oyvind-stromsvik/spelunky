﻿using UnityEngine;

namespace Spelunky {

    /// <summary>
    /// A pushable block that can crush the player.
    /// </summary>
    public class Block : PhysicsBody, IPushable {

        public AudioClip landClip;

        private bool _initialized;

        protected override void Awake() {
            base.Awake();

            // Blocks don't bounce or have friction.
            bounces = false;
            applyFriction = false;
            stopOnGround = true;
        }

        protected override void Update() {
            // Horizontal movement is handled by push-on-collision.
            velocity.x = 0f;

            base.Update();
        }

        protected override void OnPhysicsCollisionEnter(CollisionInfo collisionInfo) {
            HandleImpactDamage(collisionInfo);

            // Skip first collision to avoid landClip playing on level start for all blocks.
            if (!_initialized) {
                _initialized = true;
                return;
            }

            bool crushed = TryCrushOnVerticalCollision(collisionInfo);
            if (collisionInfo.becameGroundedThisFrame && !crushed) {
                audioSource.clip = landClip;
                audioSource.Play();
            }
        }

        public bool TryPush(Vector2Int step) {
            if (!Physics.collisionInfo.down) {
                return false;
            }

            Physics.Move(step);

            if (step.x > 0 && Physics.collisionInfo.right || step.x < 0 && Physics.collisionInfo.left) {
                TryCrushBlockedTarget(step);
                return false;
            }

            return true;
        }

        private void TryCrushBlockedTarget(Vector2Int step) {
            Collider2D blocker = Physics.collisionInfo.colliderHorizontal;
            if (blocker == null) {
                return;
            }

            ICrushable crushable = blocker.GetComponentInParent<ICrushable>();
            if (crushable == null || !crushable.IsCrushable) {
                return;
            }

            EntityPhysics blockerPhysics = blocker.GetComponentInParent<EntityPhysics>();
            if (blockerPhysics != null && !IsBlocked(blockerPhysics, step)) {
                return;
            }

            crushable.Crush();
        }

        private bool TryCrushOnVerticalCollision(CollisionInfo collisionInfo) {
            int gravityDirection = PhysicsManager.gravity.y > 0f ? 1 : (PhysicsManager.gravity.y < 0f ? -1 : 0);
            if (gravityDirection == 0) {
                return false;
            }

            bool movingWithGravity = gravityDirection > 0 ? velocity.y > 0f : velocity.y < 0f;
            if (!movingWithGravity) {
                return false;
            }

            bool hitInGravityDirection = gravityDirection > 0 ? collisionInfo.up : collisionInfo.down;
            if (!hitInGravityDirection) {
                return false;
            }

            Collider2D hit = collisionInfo.colliderVertical;
            if (hit == null) {
                return false;
            }

            ICrushable crushable = hit.GetComponentInParent<ICrushable>();
            if (crushable == null || !crushable.IsCrushable) {
                return false;
            }

            crushable.Crush();
            return true;
        }

        private static bool IsBlocked(EntityPhysics otherPhysics, Vector2Int step) {
            if (step.x > 0 && otherPhysics.collisionInfo.right) {
                return true;
            }
            if (step.x < 0 && otherPhysics.collisionInfo.left) {
                return true;
            }
            if (step.y > 0 && otherPhysics.collisionInfo.up) {
                return true;
            }
            if (step.y < 0 && otherPhysics.collisionInfo.down) {
                return true;
            }
            return false;
        }
        
    }

}
