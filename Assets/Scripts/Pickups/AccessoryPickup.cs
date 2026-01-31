using UnityEngine;

namespace Spelunky {

    public class AccessoryPickup : MonoBehaviour {

        public AccessoryType accessoryType;

        [Tooltip("Icon to show in the UI. If not set, uses the SpriteRenderer's sprite.")]
        public Sprite icon;

        private void OnTriggerEnter2D(Collider2D other) {
            var player = other.GetComponentInParent<Player>();
            if (player != null) {
                Sprite uiIcon = icon;
                if (uiIcon == null) {
                    var spriteRenderer = GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null) {
                        uiIcon = spriteRenderer.sprite;
                    }
                }

                player.Accessories.AddAccessory(accessoryType, uiIcon);
                Destroy(gameObject);
            }
        }

    }

}
