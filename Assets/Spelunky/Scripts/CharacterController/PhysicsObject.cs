using System;
using UnityEngine;

namespace Spelunky {

    [RequireComponent(typeof(IObjectController), typeof(BoxCollider2D))]
    public class PhysicsObject : MonoBehaviour {

        private struct RaycastOrigins {
            public Vector2 topLeft, topRight;
            public Vector2 bottomLeft, bottomRight;
        }

        public BoxCollider2D Collider { get; private set; }

        public bool useFixedUpdate;

        public CollisionInfo collisionInfo;
        public LayerMask collisionMask;
        public float skinWidth;
        public int horizontalRayCount;
        public int verticalRayCount;

        public bool raycastsHitTriggers;

        private float _horizontalRaySpacing;
        private float _verticalRaySpacing;
        private RaycastOrigins _raycastOrigins;
        private IObjectController _controllerInterface;

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
            _controllerInterface = GetComponent<IObjectController>();
            if (_controllerInterface == null) {
                Debug.LogError("No controller interface assigned to physics object.");
            }

            Collider = GetComponent<BoxCollider2D>();
        }

        private void Start() {
            CalculateRaySpacing();
        }

        public void Move(Vector2 velocity) {
            // Some form of terminal velocity.
            velocity.x = Mathf.Clamp(velocity.x, PhysicsManager.gravity.y, -PhysicsManager.gravity.y);
            velocity.y = Mathf.Clamp(velocity.y, PhysicsManager.gravity.y, -PhysicsManager.gravity.y);

            // Save off our current grounded state which we will use for wasGroundedLastFrame and becameGroundedThisFrame.
            collisionInfo.wasGroundedLastFrame = collisionInfo.down;
            collisionInfo.collidedLastFrame = collisionInfo.down || collisionInfo.right || collisionInfo.left || collisionInfo.up;

            UpdateRaycastOrigins();

            collisionInfo.Reset();

            if (velocity.x != 0) {
                HorizontalCollisions(ref velocity);
            }

            if (velocity.y != 0) {
                VerticalCollisions(ref velocity);
            }

            transform.Translate(velocity);

            // Set our becameGrounded state based on the previous and current collision state.
            if (!collisionInfo.wasGroundedLastFrame && collisionInfo.down) {
                collisionInfo.becameGroundedThisFrame = true;
            }
            collisionInfo.collidedThisFrame = collisionInfo.down || collisionInfo.right || collisionInfo.left || collisionInfo.up;
        }

        private Vector2 _velocity;

        private void FixedUpdate() {
            if (!useFixedUpdate) {
                return;
            }

            _controllerInterface.UpdateVelocity(ref _velocity);

            _velocity *= Time.deltaTime;

            // Some form of terminal velocity.
            _velocity.x = Mathf.Clamp(_velocity.x, PhysicsManager.gravity.y, -PhysicsManager.gravity.y);
            _velocity.y = Mathf.Clamp(_velocity.y, PhysicsManager.gravity.y, -PhysicsManager.gravity.y);

            // Save off our current grounded state which we will use for wasGroundedLastFrame and becameGroundedThisFrame.
            collisionInfo.wasGroundedLastFrame = collisionInfo.down;
            collisionInfo.collidedLastFrame = collisionInfo.down || collisionInfo.right || collisionInfo.left || collisionInfo.up;

            UpdateRaycastOrigins();

            collisionInfo.Reset();

            // Because we're handling horizontal collisions first this means that if we're
            // colliding with something both horizontally and vertically at the same time
            // the collider we'll save in the collisionInfo will be the vertical one.
            // I guess we'll see if this works fine or not.
            if (_velocity.x != 0) {
                HorizontalCollisions(ref _velocity);
            }
            if (_velocity.y != 0) {
                VerticalCollisions(ref _velocity);
            }

            transform.Translate(_velocity);

            // Set our becameGrounded state based on the previous and current collision state.
            if (!collisionInfo.wasGroundedLastFrame && collisionInfo.down) {
                collisionInfo.becameGroundedThisFrame = true;
            }
            collisionInfo.collidedThisFrame = collisionInfo.down || collisionInfo.right || collisionInfo.left || collisionInfo.up;

            if (collisionInfo.collidedThisFrame) {
                _controllerInterface.OnCollision(collisionInfo);
            }
        }

        private void HorizontalCollisions(ref Vector2 velocity) {
            float directionX = Mathf.Sign(velocity.x);
            CollisionDirection collisionDirection = velocity.x > 0 ? CollisionDirection.Right : CollisionDirection.Left;
            float rayLength = Mathf.Abs(velocity.x) + skinWidth;

            if (Mathf.Abs(velocity.x) < skinWidth) {
                rayLength = 2 * skinWidth;
            }

            for (int i = 0; i < horizontalRayCount; i++) {
                Vector2 rayOrigin = (directionX == -1) ? _raycastOrigins.bottomLeft : _raycastOrigins.bottomRight;
                rayOrigin += Vector2.up * (_horizontalRaySpacing * i);
                int hits = Physics2D.RaycastNonAlloc(rayOrigin, Vector2.right * directionX, _raycastHits, rayLength, collisionMask);
                Debug.DrawRay(rayOrigin, Vector2.right * directionX, Color.red);

                for (int j = 0; j < hits; j++) {
                    RaycastHit2D hit = _raycastHits[j];
                    if (hit) {
                        if (IgnoreCollider(hit.collider, collisionDirection)) {
                            continue;
                        }

                        velocity.x = (hit.distance - skinWidth) * directionX;
                        rayLength = hit.distance;

                        collisionInfo.left = directionX == -1;
                        collisionInfo.right = directionX == 1;
                        collisionInfo.direction = collisionDirection;
                        collisionInfo.collider = hit.collider;
                    }
                }
            }
        }

        private void VerticalCollisions(ref Vector2 velocity) {
            float directionY = Mathf.Sign(velocity.y);
            CollisionDirection collisionDirection = velocity.y > 0 ? CollisionDirection.Up : CollisionDirection.Down;
            float rayLength = Mathf.Abs(velocity.y) + skinWidth;

            for (int i = 0; i < verticalRayCount; i++) {
                Vector2 rayOrigin = (directionY == -1) ? _raycastOrigins.bottomLeft : _raycastOrigins.topLeft;
                rayOrigin += Vector2.right * (_verticalRaySpacing * i + velocity.x);
                int hits = Physics2D.RaycastNonAlloc(rayOrigin, Vector2.up * directionY, _raycastHits, rayLength, collisionMask);
                Debug.DrawRay(rayOrigin, Vector2.up * directionY, Color.red);

                for (int j = 0; j < hits; j++) {
                    RaycastHit2D hit = _raycastHits[j];
                    if (hit) {
                        if (IgnoreCollider(hit.collider, collisionDirection)) {
                            continue;
                        }

                        velocity.y = (hit.distance - skinWidth) * directionY;
                        rayLength = hit.distance;

                        collisionInfo.down = directionY == -1;
                        collisionInfo.up = directionY == 1;
                        collisionInfo.direction = collisionDirection;
                        collisionInfo.collider = hit.collider;
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

            // Allow the controller to determine if the collider should be ignored or not.
            if (_controllerInterface.IgnoreCollider(collider, direction)) {
                return true;
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
