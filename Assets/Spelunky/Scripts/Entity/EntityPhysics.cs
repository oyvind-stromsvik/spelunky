using UnityEngine;

namespace Spelunky {

    /// <summary>
    /// Our custom "Rigidbody2D" class using integer-based pixel-perfect movement.
    /// Uses Physics2D.OverlapBox for collision detection with pixel-by-pixel movement.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class EntityPhysics : MonoBehaviour {

        public CollisionInfoEvent OnCollisionEnterEvent { get; } = new CollisionInfoEvent();
        public CollisionInfoEvent OnCollisionExitEvent { get; } = new CollisionInfoEvent();

        public BoxCollider2D Collider { get; private set; }
        public Vector2 Velocity { get; private set; }
        public CollisionContext collisionContext;

        public CollisionInfo collisionInfo;
        public CollisionInfo collisionInfoLastFrame;

        public LayerMask collisionMask;
        public bool raycastsHitTriggers;

        [Header("Push Settings")]
        [Tooltip("Whether this entity can push blocks when moving horizontally.")]
        // TODO: This is currently tied to player input logic so currently it has no purpose outside of that. It would
        // make sense to change this so that it could be enabled for enemies/npcs to allow them to push blocks as well,
        // but I couldn't be bothered right now. I'm thinking this setting needs to be split up into something like
        // "Is ever allowed to push blocks" and "Can push blocks", and one is the configuration for any given entity,
        // while the other is the runtime state for the entity, or something like that.
        public bool canPushBlocks;

        // Integer position tracking
        private Vector2Int _pixelPosition;
        private Vector2 _subPixelRemainder;
        private bool _positionInitialized;

        // Collision detection cache
        private ContactFilter2D _contactFilter;
        private Collider2D[] _overlapResults = new Collider2D[16];

        private void Reset() {
            // First time I've tried setting up layermasks in code.
            // TODO: This will break if the layers change, but maybe the layers assigned in the inspector also break
            // then? Either way I will want more control over layers at some point. Like some layer manager which knows
            // all obstacle layers, all entity layers etc. etc.
            collisionMask = (1 << 8) | (1 << 12) | (1 << 13) | (1 << 15);
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
            UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(rigidbody2D, false);
            rigidbody2D.hideFlags = HideFlags.NotEditable;
#endif
        }

        private void Awake() {
            // TODO: Do the same for the collider as for the rigidbody above? We just need to expose the variables we
            // need to be able to change like size, offset, bounds etc.
            Collider = GetComponent<BoxCollider2D>();
        }

        private void Start() {
            SetupContactFilter();
        }

        private void SetupContactFilter() {
            _contactFilter = new ContactFilter2D();
            _contactFilter.useTriggers = raycastsHitTriggers;
            _contactFilter.useLayerMask = true;
            _contactFilter.layerMask = collisionMask;
        }

        private void InitializePixelPosition() {
            if (_positionInitialized) {
                return;
            }

            // Round to nearest pixel
            _pixelPosition = new Vector2Int(
                Mathf.RoundToInt(transform.position.x),
                Mathf.RoundToInt(transform.position.y)
            );

            // Push out of any initial overlaps (fixes entities spawning stuck in tiles)
            // Use a smaller overlap check to avoid false positives from edge-touching
            // Shrink by 0.5 pixels total for more reliable collision detection
            Vector2 checkSize = Collider.size - Vector2.one * 0.5f;

            int maxPushAttempts = 32;
            int pushAttempts = 0;
            while (pushAttempts < maxPushAttempts) {
                Vector2 checkPos = _pixelPosition + Collider.offset;

                int hitCount = Physics2D.OverlapBox(
                    checkPos,
                    checkSize,
                    0f,
                    _contactFilter,
                    _overlapResults
                );

                bool foundOverlap = false;
                for (int i = 0; i < hitCount; i++) {
                    Collider2D hit = _overlapResults[i];
                    if (hit == Collider) {
                        continue;
                    }

                    if (hit.isTrigger) {
                        continue;
                    }

                    if (hit.CompareTag("OneWayPlatform")) {
                        continue;
                    }

                    foundOverlap = true;
                    break;
                }

                if (!foundOverlap) {
                    break;
                }

                _pixelPosition.y += 1;
                pushAttempts++;
            }

            // Clear remainder on initialization
            _subPixelRemainder = Vector2.zero;

            // Sync transform to pixel position
            transform.position = new Vector3(_pixelPosition.x, _pixelPosition.y, transform.position.z);

            _positionInitialized = true;
        }

        /// <summary>
        /// Set the entity's position directly (for snapping to ladders, doors, ledges, etc.)
        /// Call this instead of setting transform.position directly.
        /// </summary>
        public void SetPosition(Vector2 newPosition) {
            _pixelPosition = new Vector2Int(
                Mathf.RoundToInt(newPosition.x),
                Mathf.RoundToInt(newPosition.y)
            );

            _subPixelRemainder = new Vector2(
                newPosition.x - _pixelPosition.x,
                newPosition.y - _pixelPosition.y
            );

            transform.position = new Vector3(_pixelPosition.x, _pixelPosition.y, transform.position.z);

            _positionInitialized = true;
        }

        /// <summary>
        /// Move the entity by the provided delta and do collision detection and handling for the move.
        /// Uses pixel-by-pixel movement with Physics2D.OverlapBox for collision detection.
        /// </summary>
        /// <param name="moveDelta"></param>
        public void Move(Vector2 moveDelta) {
            InitializePixelPosition();

            collisionInfoLastFrame = collisionInfo;
            collisionInfo.Reset();

            Vector2 totalDelta = moveDelta + (Vector2)collisionContext.externalDelta;

            // Add delta to sub-pixel accumulator
            _subPixelRemainder += totalDelta;

            // Extract integer pixels to move
            Vector2Int pixelsToMove = new Vector2Int(
                Mathf.RoundToInt(_subPixelRemainder.x),
                Mathf.RoundToInt(_subPixelRemainder.y)
            );

            // Update remainder
            _subPixelRemainder -= new Vector2(pixelsToMove.x, pixelsToMove.y);

            // Move pixel-by-pixel
            MoveX(pixelsToMove.x, totalDelta.x);
            MoveY(pixelsToMove.y, totalDelta.y);

            // Sync transform to pixel position
            transform.position = new Vector3(_pixelPosition.x, _pixelPosition.y, transform.position.z);

            Vector2 resolvedVelocity = totalDelta / Time.deltaTime;
            if (collisionInfo.left || collisionInfo.right) {
                resolvedVelocity.x = 0f;
            }
            if (collisionInfo.up || collisionInfo.down) {
                resolvedVelocity.y = 0f;
            }
            Velocity = resolvedVelocity;

            // Set becameGroundedThisFrame flag
            if (!collisionInfoLastFrame.down && collisionInfo.down) {
                collisionInfo.becameGroundedThisFrame = true;
            }

            ApplyCollisionContextOverrides();

            FireCollisionEvents();
        }

        private void MoveX(int pixelsX, float moveDeltaX) {
            int direction = pixelsX != 0 ? (pixelsX > 0 ? 1 : -1) : (moveDeltaX > 0f ? 1 : -1);
            if (pixelsX == 0) {
                if (Mathf.Approximately(moveDeltaX, 0f)) {
                    return;
                }

                Vector2Int nextPosition = _pixelPosition + new Vector2Int(direction, 0);

                if (CheckCollisionX(nextPosition)) {
                    collisionInfo.left = direction == -1;
                    collisionInfo.right = direction == 1;
                }

                return;
            }

            int pixelsRemaining = Mathf.Abs(pixelsX);
            for (int i = 0; i < pixelsRemaining; i++) {
                Vector2Int nextPosition = _pixelPosition + new Vector2Int(direction, 0);

                if (CheckCollisionX(nextPosition)) {
                    if (TryPushBlock(collisionInfo.colliderHorizontal, direction)) {
                        collisionInfo.left = direction == -1;
                        collisionInfo.right = direction == 1;
                        _pixelPosition.x += direction;
                        continue;
                    }

                    collisionInfo.left = direction == -1;
                    collisionInfo.right = direction == 1;
                    break;
                }

                _pixelPosition.x += direction;
            }
        }

        private bool TryPushBlock(Collider2D hit, int direction) {
            if (!canPushBlocks) {
                return false;
            }

            if (hit == null) {
                return false;
            }

            IPushable pushable = hit.GetComponent<IPushable>();
            if (pushable == null) {
                return false;
            }

            return pushable.TryPush(new Vector2Int(direction, 0));
        }

        private void MoveY(int pixelsY, float moveDeltaY) {
            int direction = pixelsY != 0 ? (pixelsY > 0 ? 1 : -1) : (moveDeltaY > 0f ? 1 : -1);
            if (pixelsY == 0) {
                if (moveDeltaY > 0f) {
                    Vector2Int nextPosition = _pixelPosition + Vector2Int.up;
                    if (CheckCollisionY(nextPosition, 1)) {
                        collisionInfo.up = true;
                    }
                }
                else {
                    CheckGround();
                }
                return;
            }

            int pixelsRemaining = Mathf.Abs(pixelsY);
            for (int i = 0; i < pixelsRemaining; i++) {
                Vector2Int nextPosition = _pixelPosition + new Vector2Int(0, direction);

                if (CheckCollisionY(nextPosition, direction)) {
                    collisionInfo.down = direction == -1;
                    collisionInfo.up = direction == 1;
                    break;
                }

                _pixelPosition.y += direction;
            }
        }

        private void CheckGround() {
            // Check one pixel below current position
            Vector2Int checkPosition = _pixelPosition + new Vector2Int(0, -1);
            Vector2 checkPos = checkPosition + Collider.offset;
            Vector2 checkSize = Collider.size - Vector2.one * 0.5f;
            Vector2 ourBottom = checkPosition + Collider.offset - new Vector2(0, Collider.size.y / 2f);

            int hitCount = Physics2D.OverlapBox(
                checkPos,
                checkSize,
                0f,
                _contactFilter,
                _overlapResults
            );

            Collider2D dynamicHit = null;
            Collider2D staticHit = null;

            for (int i = 0; i < hitCount; i++) {
                Collider2D hit = _overlapResults[i];

                if (hit == Collider) {
                    continue;
                }

                if (!raycastsHitTriggers && hit.isTrigger) {
                    continue;
                }

                // One-way platform logic - only detect as ground if we're above it
                if (hit.CompareTag("OneWayPlatform")) {
                    if (collisionInfo.fallingThroughPlatform) {
                        continue;
                    }

                    // Check if we're above the platform (our bottom is at or above platform top)
                    float platformTop = hit.bounds.max.y;

                    // If we're below the platform, not grounded on it
                    if (ourBottom.y < platformTop - 1f) {
                        continue;
                    }
                }
                else {
                    float colliderTop = hit.bounds.max.y;
                    if (ourBottom.y < colliderTop - 1f) {
                        continue;
                    }
                }

                if (hit.GetComponentInParent<EntityPhysics>() != null) {
                    dynamicHit = hit;
                    break;
                }

                if (staticHit == null) {
                    staticHit = hit;
                }
            }

            Collider2D selectedHit = dynamicHit ?? staticHit;
            if (selectedHit != null) {
                collisionInfo.down = true;
                collisionInfo.colliderVertical = selectedHit;
            }
        }

        private bool CheckCollisionX(Vector2Int nextPosition) {
            // Check if the NEXT position would overlap any colliders
            // Use a shrunk size to detect actual overlap, not edge-touching
            Vector2 checkPosition = nextPosition + Collider.offset;
            Vector2 checkSize = Collider.size - Vector2.one * 0.5f;

            int hitCount = Physics2D.OverlapBox(
                checkPosition,
                checkSize,
                0f,
                _contactFilter,
                _overlapResults
            );

            Collider2D dynamicHit = null;
            Collider2D staticHit = null;

            for (int i = 0; i < hitCount; i++) {
                Collider2D hit = _overlapResults[i];

                if (hit == Collider) {
                    continue;
                }

                if (!raycastsHitTriggers && hit.isTrigger) {
                    continue;
                }

                // One-way platforms: always ignore for horizontal collisions
                if (hit.CompareTag("OneWayPlatform")) {
                    continue;
                }

                if (hit.GetComponentInParent<EntityPhysics>() != null) {
                    dynamicHit = hit;
                    break;
                }

                if (staticHit == null) {
                    staticHit = hit;
                }
            }

            Collider2D selectedHit = dynamicHit ?? staticHit;
            if (selectedHit != null) {
                collisionInfo.colliderHorizontal = selectedHit;
                return true;
            }

            return false;
        }

        private bool CheckCollisionY(Vector2Int nextPosition, int direction) {
            // Check if the NEXT position would overlap any colliders
            // Use a shrunk size to detect actual overlap, not edge-touching
            Vector2 checkPosition = nextPosition + Collider.offset;
            Vector2 checkSize = Collider.size - Vector2.one * 0.5f;

            int hitCount = Physics2D.OverlapBox(
                checkPosition,
                checkSize,
                0f,
                _contactFilter,
                _overlapResults
            );

            Collider2D dynamicHit = null;
            Collider2D staticHit = null;

            for (int i = 0; i < hitCount; i++) {
                Collider2D hit = _overlapResults[i];

                if (hit == Collider) {
                    continue;
                }

                if (!raycastsHitTriggers && hit.isTrigger) {
                    continue;
                }

                // One-way platform logic
                if (hit.CompareTag("OneWayPlatform")) {
                    // Always ignore when moving up (jumping from below)
                    if (direction == 1) {
                        continue;
                    }

                    // Ignore if intentionally falling through
                    if (direction == -1 && collisionInfo.fallingThroughPlatform) {
                        continue;
                    }

                    // Only collide when moving down AND our bottom WOULD BE above the platform top
                    // This ensures we only land on platforms from above, not get stuck below them
                    if (direction == -1) {
                        // Calculate where our bottom would be at the NEXT position
                        Vector2 nextBottom = nextPosition + Collider.offset - new Vector2(0, Collider.size.y / 2f);
                        float platformTop = hit.bounds.max.y;

                        // If our next bottom position would be below the platform top, ignore
                        // (We're passing through from below)
                        if (nextBottom.y < platformTop - 1f) {
                            continue;
                        }
                    }
                }

                if (hit.GetComponentInParent<EntityPhysics>() != null) {
                    dynamicHit = hit;
                    break;
                }

                if (staticHit == null) {
                    staticHit = hit;
                }
            }

            Collider2D selectedHit = dynamicHit ?? staticHit;
            if (selectedHit != null) {
                collisionInfo.colliderVertical = selectedHit;
                return true;
            }

            return false;
        }

        private void FireCollisionEvents() {
            // Check if any new collision started
            bool anyCollisionEntered =
                (!collisionInfoLastFrame.left && collisionInfo.left) ||
                (!collisionInfoLastFrame.right && collisionInfo.right) ||
                (!collisionInfoLastFrame.down && collisionInfo.down) ||
                (!collisionInfoLastFrame.up && collisionInfo.up);

            if (anyCollisionEntered) {
                OnCollisionEnterEvent?.Invoke(collisionInfo);
            }

            // Check if any collision ended
            bool anyCollisionExited =
                (collisionInfoLastFrame.left && !collisionInfo.left) ||
                (collisionInfoLastFrame.right && !collisionInfo.right) ||
                (collisionInfoLastFrame.down && !collisionInfo.down) ||
                (collisionInfoLastFrame.up && !collisionInfo.up);

            if (anyCollisionExited) {
                OnCollisionExitEvent?.Invoke(collisionInfoLastFrame);
            }
        }

        private void ApplyCollisionContextOverrides() {
            if (collisionContext.groundedOverride && collisionContext.groundColliderOverride != null) {
                collisionInfo.down = true;
                collisionInfo.colliderVertical = collisionContext.groundColliderOverride;

                if (collisionInfo.colliderHorizontal == collisionContext.groundColliderOverride) {
                    collisionInfo.left = false;
                    collisionInfo.right = false;
                    collisionInfo.colliderHorizontal = null;
                }
            }

            collisionContext.Reset();
        }

    }

}
