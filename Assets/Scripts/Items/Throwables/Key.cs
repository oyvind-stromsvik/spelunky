using UnityEngine;

namespace Spelunky {

    /// <summary>
    /// A throwable key that can unlock chests when the player holds it
    /// and walks over a locked chest.
    /// </summary>
    public class Key : ThrowableItem {

        public AudioClip unlockSound;

        /// <summary>
        /// Called by Chest when this key is used to unlock it.
        /// </summary>
        public void UseKey(Player player) {
            PlayUnlockSound();

            // Remove from player's hands.
            if (_isHeld && player != null) {
                player.Holding.Drop();
            }

            Destroy(gameObject);
        }

        private void PlayUnlockSound() {
            if (unlockSound != null && AudioManager.Instance != null) {
                AudioManager.Instance.PlaySoundAtPosition(unlockSound, transform.position, AudioManager.AudioGroup.SFX);
            }
        }

    }

}
