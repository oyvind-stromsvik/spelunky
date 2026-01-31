using UnityEngine;

namespace Spelunky {

    public enum InventoryItemType {
        Bomb,
        Rope
    }

    public class InventoryPickup : MonoBehaviour {

        public InventoryItemType itemType;
        public int amount = 4;

        private void OnTriggerEnter2D(Collider2D other) {
            var player = other.GetComponentInParent<Player>();
            if (player != null) {
                switch (itemType) {
                    case InventoryItemType.Bomb:
                        player.Inventory.PickupBombs(amount);
                        break;
                    case InventoryItemType.Rope:
                        player.Inventory.PickupRopes(amount);
                        break;
                }

                Destroy(gameObject);
            }
        }

    }

}
