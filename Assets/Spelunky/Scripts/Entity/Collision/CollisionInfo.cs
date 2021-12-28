using UnityEngine;

namespace Spelunky {

    /// <summary>
    /// Info about a collision event.
    ///
    /// Similar to Collision2D for the built-in physics system.
    /// </summary>
    public struct CollisionInfo {

        public bool up;
        public bool down;
        public bool left;
        public bool right;

        // The collider we're colliding with. We can potentially collide with multiple colliders at once
        // on multiple different sides at once, but I don't think that's ever relevant? I guess we'll see.
        public CollisionDirection direction;

        public Collider2D colliderHorizontal;
        public Collider2D colliderVertical;

        public bool becameGroundedThisFrame;
        public bool fallingThroughPlatform;

        public void Reset() {
            up = false;
            down = false;
            left = false;
            right = false;
            direction = CollisionDirection.None;
            colliderHorizontal = null;
            colliderVertical = null;
            becameGroundedThisFrame = false;
        }

    }

}
