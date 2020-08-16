using UnityEngine;

namespace Spelunky {

    public abstract class Item : MonoBehaviour {

        private void OnTriggerEnter2D(Collider2D other) {
            Player player = other.GetComponent<Player>();
            if (player != null) {
                player.inventory.PickupItem(this);
                Destroy(gameObject);
            }
        }
    }
}
