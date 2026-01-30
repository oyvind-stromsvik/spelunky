using UnityEngine;

namespace Spelunky {

    /// <summary>
    /// Per-frame external motion/context for an entity before physics resolves collisions.
    /// This is a prototype container to reduce cross-system mutation of CollisionInfo.
    /// </summary>
    public struct CollisionContext {

        // External movement to apply before collision resolution (platform carry, attachments).
        public Vector2Int externalDelta;

        // Overrides for grounding when carried by a platform.
        public bool groundedOverride;
        public Collider2D groundColliderOverride;

        // Attachment information (ledge hang, moving platform hang).
        public bool isAttached;
        public MovingPlatform attachedPlatform;

        public void Reset() {
            externalDelta = Vector2Int.zero;
            groundedOverride = false;
            groundColliderOverride = null;
            isAttached = false;
            attachedPlatform = null;
        }

    }

}

