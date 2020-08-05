using UnityEngine;

namespace Spelunky {
    public struct CollisionInfo {
        public bool above;
        public bool below;
        public bool left;
        public bool right;
        public Collider2D colliderAbove;
        public Collider2D colliderBelow;
        public Collider2D colliderLeft;
        public Collider2D colliderRight;
        public bool becameGroundedThisFrame;
        public bool wasGroundedLastFrame;
        public bool collidedLastFrame;
        public bool collidedThisFrame;

        public bool fallingThroughPlatform;

        public void Reset() {
            above = false;
            below = false;
            left = false;
            right = false;
            colliderAbove = null;
            colliderBelow = null;
            colliderLeft = null;
            colliderRight = null;
            becameGroundedThisFrame = false;
            collidedThisFrame = true;
        }
    }
}
