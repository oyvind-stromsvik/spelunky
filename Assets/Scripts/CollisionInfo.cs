using UnityEngine;

public struct CollisionInfo {
    public bool above;
    public bool below;
    public bool left;
    public bool right;
    public bool becameGroundedThisFrame;
    public bool wasGroundedLastFrame;
    public bool collidedLastFrame;
    public bool collidedThisFrame;
    public Collider2D colliderBelow;

    public int faceDir;
    public bool fallingThroughPlatform;

    public void Reset() {
        above = false;
        below = false;
        left = false;
        right = false;
        becameGroundedThisFrame = false;
        collidedThisFrame = true;
        colliderBelow = null;
    }
}
