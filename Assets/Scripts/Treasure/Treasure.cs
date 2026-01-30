using UnityEngine;

namespace Spelunky {

    /// <summary>
    /// Collectible treasure that can be picked up by the player.
    /// </summary>
    public class Treasure : PhysicsBody, ICrushable {

        public AudioClip pickUpSound;
        public int value;

        [Header("Audio")]
        public AudioClip crushClip;

        private void OnTriggerEnter2D(Collider2D other) {
            Player player = other.GetComponent<Player>();
            if (player != null) {
                player.Inventory.PickupGold(value);
                AudioManager.Instance.PlaySoundAtPosition(pickUpSound, transform.position, AudioManager.AudioGroup.SFX);
                Destroy(gameObject);
            }
        }

        public bool IsCrushable => true;

        public void Crush() {
            PlayCrushSound();
            Destroy(gameObject);
        }

        private void PlayCrushSound() {
            if (crushClip == null) {
                return;
            }

            if (AudioManager.Instance != null) {
                AudioManager.Instance.PlaySoundAtPosition(crushClip, transform.position, AudioManager.AudioGroup.SFX);
            }
        }

    }

}
