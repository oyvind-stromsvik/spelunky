using UnityEngine;

namespace Spelunky {
    public interface IObjectController {
        bool IgnoreCollision(Collider2D collider, CollisionDirection direction);
    }
}
