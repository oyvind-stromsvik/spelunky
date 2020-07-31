using UnityEngine;

namespace Spelunky {
    public class GlovePickup : MonoBehaviour {

        private void OnTriggerEnter2D(Collider2D other) {
            Player player = other.GetComponent<Player>();
            if (player != null) {
                player.items.PickupGlove();
                Destroy(gameObject);
            }
        }
    }
}
