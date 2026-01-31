using UnityEngine;

namespace Spelunky {

    /// <summary>
    /// A throwable chest that can be unlocked by a key.
    /// When unlocked, spawns a random accessory.
    /// The chest detects when a player holding a key walks over it.
    /// </summary>
    public class Chest : ThrowableItem {

        [Header("Chest Settings")]
        public bool isLocked = true;

        [Tooltip("Accessories that can spawn when the chest is opened.")]
        public GameObject[] possibleAccessories;

        public AudioClip openSound;

        [Header("Visuals")]
        public Sprite openSprite;

        private SpriteRenderer _spriteRenderer;
        private bool _isOpen;

        protected override void Awake() {
            base.Awake();
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (!isLocked || _isOpen) {
                return;
            }

            var player = other.GetComponentInParent<Player>();
            if (player == null) {
                return;
            }

            // Check if player is holding a key.
            if (player.Holding.HeldItem is Key key) {
                Unlock(player, key);
            }
        }

        private void Unlock(Player player, Key key) {
            key.UseKey(player);
            Open();
        }

        public void Open() {
            if (_isOpen) {
                return;
            }

            _isOpen = true;
            isLocked = false;

            PlayOpenSound();
            UpdateVisuals();
            SpawnAccessory();
        }

        private void PlayOpenSound() {
            if (openSound != null && AudioManager.Instance != null) {
                AudioManager.Instance.PlaySoundAtPosition(openSound, transform.position, AudioManager.AudioGroup.SFX);
            }
        }

        private void UpdateVisuals() {
            if (_spriteRenderer != null && openSprite != null) {
                _spriteRenderer.sprite = openSprite;
            }
        }

        private void SpawnAccessory() {
            if (possibleAccessories == null || possibleAccessories.Length == 0) {
                return;
            }

            GameObject toSpawn = possibleAccessories[Random.Range(0, possibleAccessories.Length)];
            if (toSpawn != null) {
                // Spawn slightly above the chest.
                Vector3 spawnPos = transform.position + Vector3.up * 8f;
                Instantiate(toSpawn, spawnPos, Quaternion.identity);
            }
        }

    }

}
