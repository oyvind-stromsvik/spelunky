using UnityEngine;

namespace Spelunky {
    public class PhysicsObject : RaycastController {

        public CollisionInfo collisions;

        public void Move(Vector2 velocity) {
            // Some form of terminal velocity.
            velocity.x = Mathf.Clamp(velocity.x, PhysicsManager.gravity.y, -PhysicsManager.gravity.y);
            velocity.y = Mathf.Clamp(velocity.y, PhysicsManager.gravity.y, -PhysicsManager.gravity.y);

            // Save off our current grounded state which we will use for wasGroundedLastFrame and becameGroundedThisFrame.
            collisions.wasGroundedLastFrame = collisions.below;
            collisions.collidedLastFrame = collisions.below || collisions.right || collisions.left || collisions.above;

            UpdateRaycastOrigins();

            collisions.Reset();

            if (velocity.x != 0) {
                HorizontalCollisions(ref velocity);
            }

            if (velocity.y != 0) {
                VerticalCollisions(ref velocity);
            }

            transform.Translate(velocity);

            // Set our becameGrounded state based on the previous and current collision state.
            if (!collisions.wasGroundedLastFrame && collisions.below) {
                collisions.becameGroundedThisFrame = true;
            }
            collisions.collidedThisFrame = collisions.below || collisions.right || collisions.left || collisions.above;
        }

        private void HorizontalCollisions(ref Vector2 velocity) {
            float directionX = Mathf.Sign(velocity.x);
            float rayLength = Mathf.Abs(velocity.x) + skinWidth;

            if (Mathf.Abs(velocity.x) < skinWidth) {
                rayLength = 2 * skinWidth;
            }

            for (int i = 0; i < horizontalRayCount; i++) {
                Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
                rayOrigin += Vector2.up * (horizontalRaySpacing * i);
                RaycastHit2D[] hits = Physics2D.RaycastAll(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

                Debug.DrawRay(rayOrigin, Vector2.right * directionX, Color.red);

                foreach (RaycastHit2D hit in hits) {
                    if (hit) {
                        if (hit.collider == collider) {
                            continue;
                        }

                        if (hit.collider.CompareTag("OneWayPlatform")) {
                            continue;
                        }

                        if (hit.distance == 0) {
                            continue;
                        }

                        velocity.x = (hit.distance - skinWidth) * directionX;
                        rayLength = hit.distance;

                        collisions.left = directionX == -1;
                        collisions.right = directionX == 1;

                        if (collisions.left) {
                            collisions.colliderLeft = hit.collider;
                        }
                        if (collisions.right) {
                            collisions.colliderRight = hit.collider;
                        }
                    }
                }
            }
        }

        private void VerticalCollisions(ref Vector2 velocity) {
            float directionY = Mathf.Sign(velocity.y);
            float rayLength = Mathf.Abs(velocity.y) + skinWidth;

            for (int i = 0; i < verticalRayCount; i++) {
                Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
                rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
                RaycastHit2D[] hits = Physics2D.RaycastAll(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

                Debug.DrawRay(rayOrigin, Vector2.up * directionY, Color.red);

                foreach (RaycastHit2D hit in hits) {
                    if (hit) {
                        if (hit.collider == collider) {
                            continue;
                        }

                        if (hit.collider.CompareTag("OneWayPlatform")) {
                            if (directionY == 1 || hit.distance == 0) {
                                continue;
                            }

                            if (collisions.fallingThroughPlatform) {
                                continue;
                            }
                        }

                        velocity.y = (hit.distance - skinWidth) * directionY;
                        rayLength = hit.distance;

                        collisions.below = directionY == -1;
                        collisions.above = directionY == 1;

                        if (collisions.below) {
                            collisions.colliderBelow = hit.collider;
                        }
                        if (collisions.above) {
                            collisions.colliderAbove = hit.collider;
                        }
                    }
                }
            }
        }
    }
}
