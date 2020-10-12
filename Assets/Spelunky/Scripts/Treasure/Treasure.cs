using UnityEngine;

namespace Spelunky {

    [RequireComponent(typeof(EntityPhysics))]
    public class Treasure : MonoBehaviour {

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
