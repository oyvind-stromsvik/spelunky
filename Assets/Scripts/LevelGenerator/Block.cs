using UnityEngine;

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

        public override void Tick() {
            // Horizontal movement is handled by push-on-collision.
            velocity.x = 0f;

            base.Tick();
        }

        protected override void OnPhysicsCollisionEnter(CollisionInfo collisionInfo) {
            // Skip first collision to avoid landClip playing on level start for all blocks.
            if (!_initialized) {
                _initialized = true;
                return;
            }

            if (collisionInfo.becameGroundedThisFrame) {
                audioSource.clip = landClip;
                audioSource.Play();
            }
        }

        protected override void OnPhysicsOverlapEnter(Collider2D other) {
            base.OnPhysicsOverlapEnter(other);
            TryCrushOverlappingTarget(other);
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

        private void TryCrushOverlappingTarget(Collider2D other) {
            // Only crush when moving with gravity (falling).
            int gravityDirection = PhysicsManager.gravity.y > 0f ? 1 : (PhysicsManager.gravity.y < 0f ? -1 : 0);
            if (gravityDirection == 0) {
                return;
            }

            bool movingWithGravity = gravityDirection > 0 ? velocity.y > 0f : velocity.y < 0f;
            if (!movingWithGravity) {
                return;
            }

            ICrushable crushable = other.GetComponentInParent<ICrushable>();
            if (crushable == null || !crushable.IsCrushable) {
                return;
            }

            crushable.Crush();
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
