using UnityEngine;

namespace Spelunky {

    /// <summary>
    /// Collectible treasure that can be picked up by the player.
    /// </summary>
    public class Treasure : PhysicsBody {

        public AudioClip pickUpSound;
        public int value;

        private void OnTriggerEnter2D(Collider2D other) {
            Player player = other.GetComponent<Player>();
            if (player != null) {
                player.Inventory.PickupGold(value);
                AudioManager.Instance.PlaySoundAtPosition(pickUpSound, transform.position, AudioManager.AudioGroup.SFX);
                Destroy(gameObject);
            }
        }

    }

}
