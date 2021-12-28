using System;
using UnityEngine;
using UnityEngine.Events;

namespace Spelunky {

    [Serializable]
    public class CollisionInfoEvent : UnityEvent<CollisionInfo> {
    }

    [RequireComponent(typeof(BoxCollider2D))]
    public class EntityPhysics : MonoBehaviour {

        public CollisionInfoEvent OnCollisionEnterEvent { get; private set; } = new CollisionInfoEvent();
        public CollisionInfoEvent OnCollisionExitEvent { get; private set; } = new CollisionInfoEvent();

        private struct RaycastOrigins {
            public Vector2 topLeft, topRight;
            public Vector2 bottomLeft, bottomRight;
        }

        public BoxCollider2D Collider { get; private set; }

        public CollisionInfo collisionInfo;
        public CollisionInfo collisionInfoLastFrame;

        public LayerMask collisionMask;
        public float skinWidth;
        public int horizontalRayCount;
        public int verticalRayCount;

        public bool raycastsHitTriggers;

        private float _horizontalRaySpacing;
        private float _verticalRaySpacing;
        private RaycastOrigins _raycastOrigins;

        // The max number of colliders that our raycasts will register hits with at once.
        private const int MaxCollisions = 32;
        private static RaycastHit2D[] _raycastHits = new RaycastHit2D[MaxCollisions];

        private void Reset() {
            collisionMask = -1;
            skinWidth = 0.4f;
            horizontalRayCount = 4;
            verticalRayCount = 4;
            raycastsHitTriggers = false;
        }

        private void Awake() {
            Collider = GetComponent<BoxCollider2D>();
        }

        private void Start() {
            CalculateRaySpacing();
        }

        public void Move(Vector2 moveDelta) {
            collisionInfoLastFrame = collisionInfo;

            UpdateRaycastOrigins();

            collisionInfo.Reset();

            if (moveDelta.x != 0) {
                HorizontalCollisions(ref moveDelta);
            }

            if (moveDelta.y != 0) {
                VerticalCollisions(ref moveDelta);
            }

            transform.Translate(moveDelta);

            // Set our becameGrounded state based on the previous and current collision state.
            if (!collisionInfoLastFrame.down && collisionInfo.down) {
                collisionInfo.becameGroundedThisFrame = true;
            }

            if ((!collisionInfoLastFrame.left && collisionInfo.left) || (!collisionInfoLastFrame.right && collisionInfo.right) || (!collisionInfoLastFrame.down && collisionInfo.down) || (!collisionInfoLastFrame.up && collisionInfo.up)) {
                OnCollisionEnterEvent?.Invoke(collisionInfo);
            }

            if ((collisionInfoLastFrame.left && !collisionInfo.left) || (collisionInfoLastFrame.right && !collisionInfo.right) || (collisionInfoLastFrame.down && !collisionInfo.down) || (collisionInfoLastFrame.up && !collisionInfo.up)) {
                OnCollisionExitEvent?.Invoke(collisionInfoLastFrame);
            }
        }

        private void HorizontalCollisions(ref Vector2 moveDelta) {
            float directionX = Mathf.Sign(moveDelta.x);
            CollisionDirection collisionDirection = moveDelta.x > 0 ? CollisionDirection.Right : CollisionDirection.Left;
            float rayLength = Mathf.Abs(moveDelta.x) + skinWidth;

            if (Mathf.Abs(moveDelta.x) < skinWidth) {
                rayLength = 2 * skinWidth;
            }

            for (int i = 0; i < horizontalRayCount; i++) {
                Vector2 rayOrigin = directionX == -1 ? _raycastOrigins.bottomLeft : _raycastOrigins.bottomRight;
                rayOrigin += Vector2.up * (_horizontalRaySpacing * i);
                int hits = Physics2D.RaycastNonAlloc(rayOrigin, Vector2.right * directionX, _raycastHits, rayLength, collisionMask);
                Debug.DrawRay(rayOrigin, Vector2.right * directionX, Color.red);

                for (int j = 0; j < hits; j++) {
                    RaycastHit2D hit = _raycastHits[j];
                    if (hit) {
                        if (IgnoreCollider(hit.collider, collisionDirection)) {
                            continue;
                        }

                        moveDelta.x = (hit.distance - skinWidth) * directionX;
                        rayLength = hit.distance;

                        collisionInfo.left = directionX == -1;
                        collisionInfo.right = directionX == 1;
                        collisionInfo.direction = collisionDirection;
                        collisionInfo.colliderHorizontal = hit.collider;
                    }
                }
            }
        }

        private void VerticalCollisions(ref Vector2 moveDelta) {
            float directionY = Mathf.Sign(moveDelta.y);
            CollisionDirection collisionDirection = moveDelta.y > 0 ? CollisionDirection.Up : CollisionDirection.Down;
            float rayLength = Mathf.Abs(moveDelta.y) + skinWidth;

            for (int i = 0; i < verticalRayCount; i++) {
                Vector2 rayOrigin = directionY == -1 ? _raycastOrigins.bottomLeft : _raycastOrigins.topLeft;
                rayOrigin += Vector2.right * (_verticalRaySpacing * i + moveDelta.x);
                int hits = Physics2D.RaycastNonAlloc(rayOrigin, Vector2.up * directionY, _raycastHits, rayLength, collisionMask);
                Debug.DrawRay(rayOrigin, Vector2.up * directionY, Color.red);

                for (int j = 0; j < hits; j++) {
                    RaycastHit2D hit = _raycastHits[j];
                    if (hit) {
                        if (IgnoreCollider(hit.collider, collisionDirection)) {
                            continue;
                        }

                        moveDelta.y = (hit.distance - skinWidth) * directionY;
                        rayLength = hit.distance;

                        collisionInfo.down = directionY == -1;
                        collisionInfo.up = directionY == 1;
                        collisionInfo.direction = collisionDirection;
                        collisionInfo.colliderVertical = hit.collider;
                    }
                }
            }
        }

        private bool IgnoreCollider(Collider2D collider, CollisionDirection direction) {
            if (raycastsHitTriggers == false && collider.isTrigger) {
                return true;
            }

            // If the collider we hit is ourself, ignore it.
            if (collider == Collider) {
                return true;
            }

            // One way platform handling.
            if (collider.CompareTag("OneWayPlatform")) {
                // Always ignore them if we're colliding horizontally.
                if (direction == CollisionDirection.Left || direction == CollisionDirection.Right) {
                    return true;
                }

                // If we're colliding vertically ignore them if we're going up or if we're passing through them.
                if (direction == CollisionDirection.Up || direction == CollisionDirection.Down) {
                    if (direction == CollisionDirection.Up) {
                        return true;
                    }

                    if (collisionInfo.fallingThroughPlatform) {
                        return true;
                    }
                }
            }

            return false;
        }

        private void UpdateRaycastOrigins() {
            Bounds bounds = Collider.bounds;
            bounds.Expand(skinWidth * -2);

            _raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
            _raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
            _raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
            _raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
        }

        private void CalculateRaySpacing() {
            Bounds bounds = Collider.bounds;
            bounds.Expand(skinWidth * -2);
            _horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
            _verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
        }

    }

}