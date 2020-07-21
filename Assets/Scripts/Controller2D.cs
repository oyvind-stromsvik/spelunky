using UnityEngine;

public class Controller2D : RaycastController {

    public CollisionInfo collisions;

    public override void Start() {
        base.Start();
        collisions.faceDir = 1;
    }

    public void Move(Vector2 moveAmount) {
        // Save off our current grounded state which we will use for wasGroundedLastFrame and becameGroundedThisFrame.
        collisions.wasGroundedLastFrame = collisions.below;
        collisions.collidedLastFrame = collisions.below || collisions.right || collisions.left || collisions.above;

        UpdateRaycastOrigins();

        collisions.Reset();

        if (moveAmount.x != 0) {
            collisions.faceDir = (int) Mathf.Sign(moveAmount.x);
        }

        HorizontalCollisions(ref moveAmount);
        if (moveAmount.y != 0) {
            VerticalCollisions(ref moveAmount);
        }

        transform.Translate(moveAmount);

        // Set our becameGrounded state based on the previous and current collision state.
        if (!collisions.wasGroundedLastFrame && collisions.below) {
            collisions.becameGroundedThisFrame = true;
        }
        collisions.collidedThisFrame = collisions.below || collisions.right || collisions.left || collisions.above;
    }

    private void HorizontalCollisions(ref Vector2 moveAmount) {
        float directionX = collisions.faceDir;
        float rayLength = Mathf.Abs(moveAmount.x) + skinWidth;

        if (Mathf.Abs(moveAmount.x) < skinWidth) {
            rayLength = 2 * skinWidth;
        }

        for (int i = 0; i < horizontalRayCount; i++) {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.right * directionX, Color.red);

            if (hit) {
                if (hit.distance == 0) {
                    continue;
                }

                moveAmount.x = (hit.distance - skinWidth) * directionX;
                rayLength = hit.distance;

                collisions.left = directionX == -1;
                collisions.right = directionX == 1;
            }
        }
    }

    private void VerticalCollisions(ref Vector2 moveAmount) {
        float directionY = Mathf.Sign(moveAmount.y);
        float rayLength = Mathf.Abs(moveAmount.y) + skinWidth;

        for (int i = 0; i < verticalRayCount; i++) {
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + moveAmount.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.up * directionY, Color.red);

            if (hit) {
                if (hit.collider.CompareTag("OneWayPlatform")) {
                    if (directionY == 1 || hit.distance == 0) {
                        continue;
                    }

                    if (collisions.fallingThroughPlatform) {
                        continue;
                    }
                }

                moveAmount.y = (hit.distance - skinWidth) * directionY;
                rayLength = hit.distance;

                collisions.below = directionY == -1;
                collisions.above = directionY == 1;

                collisions.colliderBelow = hit.collider;
            }
        }
    }
}
