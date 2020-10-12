using UnityEngine;

namespace Spelunky {

    public struct CollisionInfo {
        public bool up;
        public bool down;
        public bool left;

        public bool right;

        // The collider we're colliding with. We can potentially collide with multiple colliders at once
        // on multiple different sides at once, but I don't think that's ever relevant? I guess we'll see.
        public CollisionDirection direction;
        public Collider2D collider;
        public bool becameGroundedThisFrame;
        public bool wasGroundedLastFrame;
        public bool collidedLastFrame;
        public bool collidedThisFrame;

        public bool fallingThroughPlatform;

        public void Reset() {
            up = false;
            down = false;
            left = false;
            right = false;
            direction = CollisionDirection.None;
            collider = null;
            becameGroundedThisFrame = false;
            collidedThisFrame = true;
        }
    }

}