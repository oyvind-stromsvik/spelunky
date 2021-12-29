using UnityEngine;

namespace Spelunky {

    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
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

            ValidateData();
        }

        private void OnValidate() {
            ValidateData();
        }

        private void ValidateData() {
            // Even though we're doing the full collision detection and handling ourselves a rigidbody is required for
            // Unity to even allow colliders to detect triggers and we use triggers for various things. So we add a
            // rigidbody, but we make sure it's impossible to actually interact with it.
            Rigidbody2D rigidbody2D = GetComponent<Rigidbody2D>();
            // Ensure the rigidbody doesn't actually affect us.
            rigidbody2D.isKinematic = true;
            // Disable and collapse the inspector.
            rigidbody2D.hideFlags = HideFlags.NotEditable;
            UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(rigidbody2D, false);
        }

        private void Awake() {
            // TODO: Do the same for the collider as for the rigidbody above? We just need to expose the variables we
            // need to be able to change like size, offset, bounds etc.
            Collider = GetComponent<BoxCollider2D>();
        }

        private void Start() {
            CalculateRaySpacing();
        }

        /// <summary>
        /// Move the entity by the provided delta and do collision detection and handling for the move.
        ///
        /// NOTE: Currently this is called from Update(). I want to make it so that it's called from FixedUpdate(), or
        /// maybe even so that we remove Move() altogether and take full control over the call order so that the caller
        /// can't do things in the wrong order. The reason I don't use FixedUpdate() at the moment is because I don't
        /// understand how to properly interpolate the movement.
        /// </summary>
        /// <param name="moveDelta"></param>
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

                        if (IgnoreCollider(hit.collider, directionX, "horizontal")) {
                            continue;
                        }

                        collisionInfo.left = directionX == -1;
                        collisionInfo.right = directionX == 1;
                        collisionInfo.colliderHorizontal = hit.collider;

                        moveDelta.x = (hit.distance - skinWidth) * directionX;
                        rayLength = hit.distance;
                    }
                }
            }
        }

        private void VerticalCollisions(ref Vector2 moveDelta) {
            float directionY = Mathf.Sign(moveDelta.y);
            float rayLength = Mathf.Abs(moveDelta.y) + skinWidth;

            for (int i = 0; i < verticalRayCount; i++) {
                Vector2 rayOrigin = directionY == -1 ? _raycastOrigins.bottomLeft : _raycastOrigins.topLeft;
                rayOrigin += Vector2.right * (_verticalRaySpacing * i + moveDelta.x);
                int hits = Physics2D.RaycastNonAlloc(rayOrigin, Vector2.up * directionY, _raycastHits, rayLength, collisionMask);
                Debug.DrawRay(rayOrigin, Vector2.up * directionY, Color.red);

                for (int j = 0; j < hits; j++) {
                    RaycastHit2D hit = _raycastHits[j];
                    if (hit) {
                        if (IgnoreCollider(hit.collider, directionY, "vertical")) {
                            continue;
                        }

                        collisionInfo.down = directionY == -1;
                        collisionInfo.up = directionY == 1;
                        collisionInfo.colliderVertical = hit.collider;

                        moveDelta.y = (hit.distance - skinWidth) * directionY;
                        rayLength = hit.distance;
                    }
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="collider">The collider to check if we want to ignore a collision with or not.</param>
        /// <param name="direction">A signed value indicating the direction.</param>
        /// <param name="type">Whether we were called from the horizontal or the vertical collision check.</param>
        /// <returns>TRUE if we should ignore the collider, FALSE otherwise.</returns>
        private bool IgnoreCollider(Collider2D collider, float direction, string type) {
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
                if (type == "horizontal") {
                    return true;
                }

                // If we're colliding vertically...
                if (type == "vertical") {
                    /// ignore them if we're going up...
                    if (direction == 1) {
                        return true;
                    }

                    /// or if we're going down and flagged to pass through them.
                    if (direction == -1 && collisionInfo.fallingThroughPlatform) {
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
