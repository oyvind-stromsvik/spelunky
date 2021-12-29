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

        public Collider2D colliderHorizontal;
        public Collider2D colliderVertical;

        public bool becameGroundedThisFrame;
        public bool fallingThroughPlatform;

        public void Reset() {
            up = false;
            down = false;
            left = false;
            right = false;
            colliderHorizontal = null;
            colliderVertical = null;
            becameGroundedThisFrame = false;
        }

    }

}
