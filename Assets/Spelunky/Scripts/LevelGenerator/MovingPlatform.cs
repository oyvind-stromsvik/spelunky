using System.Collections.Generic;
using UnityEngine;

namespace Spelunky {

    /// <summary>
    /// A pixel-perfect moving platform that carries entities and can squish them against solids.
    /// Moves along a single axis until blocked by static colliders, then reverses.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class MovingPlatform : MonoBehaviour {

        public enum StartDirection {
            Up,
            Down,
            Left,
            Right
        }

        [Header("Movement")]
        public StartDirection startDirection = StartDirection.Right;
        public float speed = 32f;

        [Header("Behavior")]
        public bool isActive = true;
        public LayerMask riderMask = ~0;
        public LayerMask obstacleMask = ~0;

        private const float TopSurfaceTolerance = 0.5f;

        private BoxCollider2D _collider;
        private Vector2Int _pixelPosition;
        private Vector2 _subPixelRemainder;
        private Vector2Int _direction;

        public Vector2Int LastStep { get; private set; }

        private ContactFilter2D _contactFilter;
        private ContactFilter2D _obstacleFilter;
        private readonly Collider2D[] _overlapResults = new Collider2D[16];
        private readonly List<EntityPhysics> _ridersOnTop = new List<EntityPhysics>(8);
        private readonly List<EntityPhysics> _attachedEntities = new List<EntityPhysics>(4);
        private readonly HashSet<EntityPhysics> _movedEntities = new HashSet<EntityPhysics>();

        private void Awake() {
            _collider = GetComponent<BoxCollider2D>();

            Vector2 startPoint = RoundToPixel(transform.position);
            _pixelPosition = new Vector2Int(Mathf.RoundToInt(startPoint.x), Mathf.RoundToInt(startPoint.y));
            _subPixelRemainder = Vector2.zero;
            _direction = DirectionFromStart(startDirection);

            SetupContactFilter();
            SyncTransform();
        }

        private void OnValidate() {
            SetupContactFilter();
        }

        private void LateUpdate() {
            if (!isActive) {
                return;
            }

            MoveAlongPath();
        }

        private void SetupContactFilter() {
            _contactFilter = new ContactFilter2D {
                useTriggers = false,
                useLayerMask = true,
                layerMask = riderMask
            };

            _obstacleFilter = new ContactFilter2D {
                useTriggers = false,
                useLayerMask = true,
                layerMask = obstacleMask
            };
        }

        private void MoveAlongPath() {
            LastStep = Vector2Int.zero;
            Vector2 desiredDelta = (Vector2)_direction * speed * Time.deltaTime;
            _subPixelRemainder += desiredDelta;

            Vector2Int pixelsToMove = new Vector2Int(
                Mathf.RoundToInt(_subPixelRemainder.x),
                Mathf.RoundToInt(_subPixelRemainder.y)
            );

            _subPixelRemainder -= new Vector2(pixelsToMove.x, pixelsToMove.y);

            MovePixels(pixelsToMove);
        }

        private void MovePixels(Vector2Int pixelsToMove) {
            MoveAxis(pixelsToMove.x, 0);
            MoveAxis(0, pixelsToMove.y);
            SyncTransform();
        }

        private void MoveAxis(int pixelsX, int pixelsY) {
            int steps = Mathf.Abs(pixelsX != 0 ? pixelsX : pixelsY);
            if (steps == 0) {
                return;
            }

            int stepX = pixelsX == 0 ? 0 : (pixelsX > 0 ? 1 : -1);
            int stepY = pixelsY == 0 ? 0 : (pixelsY > 0 ? 1 : -1);
            Vector2Int step = new Vector2Int(stepX, stepY);

            for (int i = 0; i < steps; i++) {
                if (IsBlockedByStatic(step)) {
                    LastStep = Vector2Int.zero;
                    ReverseDirection();
                    break;
                }

                GetRidersOnTop();

                _pixelPosition += step;
                LastStep = step;
                SyncTransform();
                HandleRidersAndOverlaps(step);
            }
        }

        private void HandleRidersAndOverlaps(Vector2Int step) {
            _movedEntities.Clear();

            for (int i = 0; i < _attachedEntities.Count; i++) {
                MoveAttachedEntity(_attachedEntities[i], step);
            }

            for (int i = 0; i < _ridersOnTop.Count; i++) {
                MoveRidingEntity(_ridersOnTop[i], step);
            }

            Vector2 overlapSize = GetOverlapSize();
            int hitCount = Physics2D.OverlapBox(
                _pixelPosition + _collider.offset,
                overlapSize,
                0f,
                _contactFilter,
                _overlapResults
            );

            if (hitCount == 0) {
                return;
            }

            for (int i = 0; i < hitCount; i++) {
                Collider2D hit = _overlapResults[i];
                if (hit == null || hit == _collider) {
                    continue;
                }

                EntityPhysics otherPhysics = hit.GetComponent<EntityPhysics>();
                if (otherPhysics == null) {
                    continue;
                }

                if (_movedEntities.Contains(otherPhysics)) {
                    continue;
                }

                MoveEntityInFront(otherPhysics, step);
            }
        }

        /// <summary>
        /// Currently for when the player is hanging from the edge of a moving platform.
        /// </summary>
        /// <param name="otherPhysics"></param>
        /// <param name="step"></param>
        private void MoveAttachedEntity(EntityPhysics otherPhysics, Vector2Int step) {
            if (otherPhysics == null) {
                return;
            }

            otherPhysics.collisionContext.isAttached = true;
            otherPhysics.collisionContext.attachedPlatform = this;
            otherPhysics.collisionContext.externalDelta = step;
            otherPhysics.Move(Vector2.zero);
        }

        /// <summary>
        /// Moves an entity that is riding on top of the platform.
        /// Can be the player or any other entity.
        /// </summary>
        /// <param name="otherPhysics"></param>
        /// <param name="step"></param>
        private void MoveRidingEntity(EntityPhysics otherPhysics, Vector2Int step) {
            _movedEntities.Add(otherPhysics);

            otherPhysics.collisionContext.groundedOverride = true;
            otherPhysics.collisionContext.groundColliderOverride = _collider;
            otherPhysics.collisionContext.externalDelta = step;
            otherPhysics.Move(Vector2.zero);
        }

        /// <summary>
        /// Moves an entity that is in front of the platform.
        /// Essentially pushing entities in front.
        /// </summary>
        /// <param name="otherPhysics"></param>
        /// <param name="step"></param>
        private void MoveEntityInFront(EntityPhysics otherPhysics, Vector2Int step) {
            // if (step.x != 0 && IsBlockInAir(otherPhysics)) {
            //     return;
            // }

            _movedEntities.Add(otherPhysics);

            otherPhysics.collisionContext.externalDelta = step;
            otherPhysics.Move(Vector2.zero);

            // if (otherPhysics.collisionInfo.colliderVertical == _collider) {
            //     otherPhysics.collisionInfo.down = false;
            //     otherPhysics.collisionInfo.colliderVertical = null;
            // }

            if (IsBlocked(otherPhysics, step)) {
                HandleBlockedEntity(otherPhysics);
            }
        }

        private void GetRidersOnTop() {
            _ridersOnTop.Clear();

            Vector2 topCheckSize = new Vector2(Mathf.Max(1f, _collider.size.x + 0.5f), 2f);
            Vector2 topCheckPos = _pixelPosition + _collider.offset + new Vector2(0f, _collider.size.y / 2f + 0.5f);

            int hitCount = Physics2D.OverlapBox(
                topCheckPos,
                topCheckSize,
                0f,
                _contactFilter,
                _overlapResults
            );

            if (hitCount == 0) {
                return;
            }

            for (int i = 0; i < hitCount; i++) {
                Collider2D hit = _overlapResults[i];
                if (hit == null || hit == _collider) {
                    continue;
                }

                EntityPhysics otherPhysics = hit.GetComponent<EntityPhysics>();
                if (otherPhysics == null) {
                    continue;
                }

                if (IsRidingPlatform(otherPhysics) || IsOnTop(otherPhysics)) {
                    _ridersOnTop.Add(otherPhysics);
                }
            }
        }

        private bool IsOnTop(EntityPhysics otherPhysics) {
            float platformTop = _collider.bounds.max.y;
            float otherBottom = otherPhysics.Collider.bounds.min.y;
            return otherBottom >= platformTop - TopSurfaceTolerance;
        }

        private bool IsRidingPlatform(EntityPhysics otherPhysics) {
            return otherPhysics.collisionInfo.down && otherPhysics.collisionInfo.colliderVertical == _collider;
        }

        private Vector2 GetOverlapSize() {
            Vector2 size = _collider.size - Vector2.one * 0.5f;
            return new Vector2(Mathf.Max(1f, size.x), Mathf.Max(1f, size.y));
        }

        private bool IsBlockedByStatic(Vector2Int step) {
            Vector2 checkPosition = _pixelPosition + step + _collider.offset;
            Vector2 checkSize = _collider.size - Vector2.one * 0.5f;

            int hitCount = Physics2D.OverlapBox(
                checkPosition,
                checkSize,
                0f,
                _obstacleFilter,
                _overlapResults
            );

            if (hitCount == 0) {
                return false;
            }

            for (int i = 0; i < hitCount; i++) {
                Collider2D hit = _overlapResults[i];
                if (hit == null || hit == _collider) {
                    continue;
                }

                if (hit.isTrigger) {
                    continue;
                }

                if (hit.GetComponent<EntityPhysics>() != null) {
                    continue;
                }

                return true;
            }

            return false;
        }

        private static Vector2Int DirectionFromStart(StartDirection direction) {
            switch (direction) {
                case StartDirection.Up:
                    return Vector2Int.up;
                case StartDirection.Down:
                    return Vector2Int.down;
                case StartDirection.Left:
                    return Vector2Int.left;
                case StartDirection.Right:
                default:
                    return Vector2Int.right;
            }
        }

        private static bool IsBlockInAir(EntityPhysics otherPhysics) {
            if (otherPhysics == null || otherPhysics.collisionInfo.down) {
                return false;
            }

            return otherPhysics.GetComponent<Block>() != null;
        }

        public void RegisterAttached(EntityPhysics otherPhysics) {
            if (otherPhysics == null) {
                return;
            }

            if (!_attachedEntities.Contains(otherPhysics)) {
                _attachedEntities.Add(otherPhysics);
            }
        }

        public void UnregisterAttached(EntityPhysics otherPhysics) {
            if (otherPhysics == null) {
                return;
            }

            _attachedEntities.Remove(otherPhysics);
        }

        /// <summary>
        /// Returns TRUE if the entity we're trying to push is colliding in the given direction,
        /// meaning it's unable to move further in the push direction.
        /// </summary>
        /// <param name="otherPhysics"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        private static bool IsBlocked(EntityPhysics otherPhysics, Vector2Int step) {
            if (step.x > 0 && otherPhysics.collisionInfo.right) {
                return true;
            }
            if (step.x < 0 && otherPhysics.collisionInfo.left) {
                return true;
            }
            if (step.y > 0 && otherPhysics.collisionInfo.up) {
                return true;
            }
            if (step.y < 0 && otherPhysics.collisionInfo.down) {
                return true;
            }
            return false;
        }

        private void HandleBlockedEntity(EntityPhysics otherPhysics) {
            ICrushable crushable = otherPhysics.GetComponent<ICrushable>();
            if (crushable != null && crushable.IsCrushable) {
                crushable.Crush();
                return;
            }
            
            ReverseDirection();
        }

        private void ReverseDirection() {
            _direction = -_direction;
            _subPixelRemainder = Vector2.zero;
        }

        private void SyncTransform() {
            transform.position = new Vector3(_pixelPosition.x, _pixelPosition.y, transform.position.z);
        }

        private static Vector2 RoundToPixel(Vector2 position) {
            return new Vector2(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
        }

    }

}
