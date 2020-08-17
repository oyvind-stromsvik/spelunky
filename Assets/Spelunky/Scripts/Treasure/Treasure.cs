using UnityEngine;

namespace Spelunky {

    [RequireComponent(typeof(PhysicsObject))]
    public class Treasure : MonoBehaviour, IObjectController {

        public int value;

        private void OnTriggerEnter2D(Collider2D other) {
            Player player = other.GetComponent<Player>();
            if (player != null) {
                player.inventory.PickupGold(value);
                Destroy(gameObject);
            }
        }

        public bool IgnoreCollider(Collider2D collider, CollisionDirection direction) {
            return false;
        }

        public void OnCollision(CollisionInfo collisionInfo) {
        }

        public void UpdateVelocity(ref Vector2 velocity) {

        }
    }
}
