using UnityEngine;

namespace Spelunky {
    public interface IObjectController {
        bool IgnoreCollider(Collider2D collider, CollisionDirection direction);
        void OnCollision(CollisionInfo collisionInfo);
        void UpdateVelocity(ref Vector2 velocity);
    }
}
