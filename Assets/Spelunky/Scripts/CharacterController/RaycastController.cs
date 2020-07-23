using UnityEngine;

namespace Spelunky {
    [RequireComponent(typeof(BoxCollider2D))]
    public class RaycastController : MonoBehaviour {

        public LayerMask collisionMask;
        public float skinWidth = 0.4f;
        public int horizontalRayCount = 4;
        public int verticalRayCount = 4;
        [HideInInspector] public float horizontalRaySpacing;
        [HideInInspector] public float verticalRaySpacing;
        [HideInInspector] public new BoxCollider2D collider;
        public RaycastOrigins raycastOrigins;

        public virtual void Awake() {
            collider = GetComponent<BoxCollider2D>();
        }

        public virtual void Start() {
            CalculateRaySpacing();
        }

        protected void UpdateRaycastOrigins() {
            Bounds bounds = collider.bounds;
            bounds.Expand(skinWidth * -2);

            raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
            raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
            raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
            raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
        }

        private void CalculateRaySpacing() {
            Bounds bounds = collider.bounds;
            bounds.Expand(skinWidth * -2);
            horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
            verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
        }

        public struct RaycastOrigins {
            public Vector2 topLeft, topRight;
            public Vector2 bottomLeft, bottomRight;
        }
    }
}
