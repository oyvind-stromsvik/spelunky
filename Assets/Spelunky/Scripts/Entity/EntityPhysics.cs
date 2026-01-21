using UnityEditorInternal;
using UnityEngine;

namespace Spelunky {

    /// <summary>
    /// Our custom "Rigidbody2D" class.
    /// TODO: Should there be a character controller wrapper class for this? So that rocks, bombs, blocks and other
    /// actual rigidbodies can have this class and then our player can have the character controller class? Also is it
    /// just ridiculous not to use the built-in Rigidbody2D for anything?
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class EntityPhysics : MonoBehaviour {

        public CollisionInfoEvent OnCollisionEnterEvent { get; } = new CollisionInfoEvent();
        public CollisionInfoEvent OnCollisionExitEvent { get; } = new CollisionInfoEvent();

        // TODO: I really want to get rid of all this raycast nonsense and just use Collider.Cast or Physics2D.Boxcast
        // instead, but they don't return precise collisions so I would have to find a workaround for that.
        // Ref. https://forum.unity.com/threads/spelunky-clone-open-source-2d-platformer.935966/#post-6172939
        private struct RaycastOrigins {

            public Vector2 topLeft;
            public Vector2 bottomLeft;
            public Vector2 bottomRight;

        }

        public BoxCollider2D Collider { get; private set; }
        public Vector2 Velocity { get; private set; }

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
            // First time I've tried setting up layermasks in code.
            // TODO: This will break if the layers change, but maybe the layers assigned in the inspector also break
            // then? Either way I will want more control over layers at some point. Like some layer manager which knows
            // all obstacle layers, all entity layers etc. etc.
            collisionMask = (1 << 8) | (1 << 12) | (1 << 13) | (1 << 15);
            skinWidth = 0.4f;
            horizontalRayCount = 2;
            verticalRayCount = 2;
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
#if UNITY_EDITOR
            rigidbody2D.hideFlags = HideFlags.NotEditable;
            InternalEditorUtility.SetIsInspectorExpanded(rigidbody2D, false);
#endif
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

            // We don't need to check horizontal collisions if there are no horizontal movement.
            if (moveDelta.x != 0) {
                HorizontalCollisions(ref moveDelta.x);
            }

            // Even if there is no vertical movement we still want to check for ground.
            VerticalCollisions(ref moveDelta.y);

            // Actually move our entity, with the movement delta adjusted based on the resolved collisions above.
            transform.Translate(moveDelta);

            Velocity = moveDelta / Time.deltaTime;

            // Set our becameGrounded state based on the previous and current collision state.
            if (!collisionInfoLastFrame.down && collisionInfo.down) {
                collisionInfo.becameGroundedThisFrame = true;
            }

            if ((!collisionInfoLastFrame.left && collisionInfo.left) || (!collisionInfoLastFrame.right && collisionInfo.right) || (!collisionInfoLastFrame.down && collisionInfo.down) || (!collisionInfoLastFrame.up && collisionInfo.up)) {
                OnCollisionEnterEvent?.Invoke(collisionInfo);
            }

            // TODO: If the collider is destroyed last frame this will cause an exception if someone tries to access it
            // in this event. Figure out how to handle that.
            if ((collisionInfoLastFrame.left && !collisionInfo.left) || (collisionInfoLastFrame.right && !collisionInfo.right) || (collisionInfoLastFrame.down && !collisionInfo.down) || (collisionInfoLastFrame.up && !collisionInfo.up)) {
                OnCollisionExitEvent?.Invoke(collisionInfoLastFrame);
            }
        }

        /// <summary>
        /// Check for and resolve any horizontal collisions for this move.
        /// </summary>
        /// <param name="moveDeltaX">The horizontal translation to check for collisions.</param>
        private void HorizontalCollisions(ref float moveDeltaX) {
            float directionX = Mathf.Sign(moveDeltaX);
            float rayLength = Mathf.Abs(moveDeltaX) + skinWidth;

            if (Mathf.Abs(moveDeltaX) < skinWidth) {
                rayLength = 2 * skinWidth;
            }

            bool resolvedCollision = false;
            for (int i = 0; i < horizontalRayCount; i++) {
                if (resolvedCollision) {
                    break;
                }

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

                        moveDeltaX = (hit.distance - skinWidth) * directionX;

                        // In Sebastian Lague's original tutorial I think he supports slopes so he has to go through all
                        // the hits because they could be different lengths due to us being on a slope. We don't support
                        // that so there's no reason to check more than one hit. We only use multiple raycasts to ensure
                        // nothing smaller than our bounds passes through us.
                        resolvedCollision = true;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Check for and resolve vertical collisions for this move.
        /// If we're not actually moving we still check to see if we're grounded without resolving any collisions.
        /// </summary>
        /// <param name="moveDeltaY">The vertical translation to check for collisions.</param>
        private void VerticalCollisions(ref float moveDeltaY) {
            bool justCheckForGround = moveDeltaY == 0;

            float directionY = justCheckForGround ? -1 : Mathf.Sign(moveDeltaY);
            float rayLength = Mathf.Abs(moveDeltaY) + skinWidth;

            if (Mathf.Abs(moveDeltaY) < skinWidth) {
                rayLength = 2 * skinWidth;
            }

            bool resolvedCollision = false;
            for (int i = 0; i < verticalRayCount; i++) {
                if (resolvedCollision) {
                    break;
                }

                Vector2 rayOrigin = directionY == -1 ? _raycastOrigins.bottomLeft : _raycastOrigins.topLeft;
                rayOrigin += Vector2.right * (_verticalRaySpacing * i);
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

                        if (!justCheckForGround) {
                            moveDeltaY = (hit.distance - skinWidth) * directionY;
                        }

                        // In Sebastian Lague's original tutorial I think he supports slopes so he has to go through all
                        // the hits because they could be different lengths due to us being on a slope. We don't support
                        // that so there's no reason to check more than one hit. We only use multiple raycasts to ensure
                        // nothing smaller than our bounds passes through us.
                        resolvedCollision = true;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Check if we should ignore collisions with the given collider.
        /// </summary>
        /// <param name="collider">The collider to check if we want to ignore a collision with or not.</param>
        /// <param name="direction">A signed value indicating the direction.</param>
        /// <param name="type">Whether we were called from the horizontal or the vertical collision check.</param>
        /// <returns>TRUE if we should ignore the collider, FALSE otherwise.</returns>
        private bool IgnoreCollider(Collider2D collider, float direction, string type) {
            if (!raycastsHitTriggers && collider.isTrigger) {
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

        /// <summary>
        /// Update the raycast origins.
        /// Because the bounds are in world space this needs to happen before every collision check.
        /// </summary>
        private void UpdateRaycastOrigins() {
            Bounds bounds = CalculateBounds();
            _raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
            _raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
            _raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        }

        /// <summary>
        /// Calculate the spacing for our raycasts based on our adjusted bounds.
        /// </summary>
        private void CalculateRaySpacing() {
            Bounds bounds = CalculateBounds();
            _horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
            _verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
        }

        /// <summary>
        /// Create bounds that are slightly smaller than our collider.
        /// Bounds need to be created before every collision check because they are in world space.
        /// TODO: I can't exactly remember why we have a value of -2 in here. Need to double check Sebastian Lague's
        /// tutorial again, I guess. Without shrinking the bounds we'll catch on edges and trigger vertical collisions
        /// when sliding along walls etc. at least which is very undesirable.
        /// TODO: But this also means we're not getting pixel perfect collisions. For example when standing on an edge
        /// we'll now fall off while our collider is still on the edge due to the bounds being shrunk. This is also not
        /// desirable.
        /// </summary>
        /// <returns></returns>
        private Bounds CalculateBounds() {
            Bounds bounds = Collider.bounds;
            bounds.Expand(skinWidth * -2);
            return bounds;
        }

    }

}