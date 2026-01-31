using UnityEngine;

namespace Spelunky {

    /// <summary>
    /// A throwable item that breaks on impact above a velocity threshold.
    /// Can optionally spawn items when broken.
    /// Used for: Skull (no spawn), Jar (spawns treasure), Crate (spawns any item).
    /// </summary>
    public class BreakableThrowable : ThrowableItem {

        [Header("Break Settings")]
        [Tooltip("Minimum velocity magnitude required to break on impact.")]
        public float breakVelocityThreshold = 128f;

        [Tooltip("Items that can spawn when broken. Leave empty for no spawn. Randomly selects one.")]
        public GameObject[] possibleSpawns;

        [Tooltip("Chance (0-1) that something spawns when broken. Only applies if possibleSpawns is not empty.")]
        [Range(0f, 1f)]
        public float spawnChance = 1f;

        [Header("Break Audio")]
        public AudioClip breakSound;

        protected override void OnPhysicsCollisionEnter(CollisionInfo collisionInfo) {
            float impactVelocity = GetImpactVelocity(collisionInfo);

            if (impactVelocity >= breakVelocityThreshold) {
                Break();
                return;
            }

            base.OnPhysicsCollisionEnter(collisionInfo);
        }

        private float GetImpactVelocity(CollisionInfo collisionInfo) {
            float impactVelocity = 0f;

            if (collisionInfo.left || collisionInfo.right) {
                impactVelocity = Mathf.Max(impactVelocity, Mathf.Abs(Velocity.x));
            }

            if (collisionInfo.up || collisionInfo.down) {
                impactVelocity = Mathf.Max(impactVelocity, Mathf.Abs(Velocity.y));
            }

            return impactVelocity;
        }

        protected virtual void Break() {
            PlayBreakSound();
            TrySpawnItem();
            Destroy(gameObject);
        }

        private void PlayBreakSound() {
            if (breakSound != null && AudioManager.Instance != null) {
                AudioManager.Instance.PlaySoundAtPosition(breakSound, transform.position, AudioManager.AudioGroup.SFX);
            }
        }

        private void TrySpawnItem() {
            if (possibleSpawns == null || possibleSpawns.Length == 0) {
                return;
            }

            if (Random.value > spawnChance) {
                return;
            }

            GameObject toSpawn = possibleSpawns[Random.Range(0, possibleSpawns.Length)];
            if (toSpawn != null) {
                Instantiate(toSpawn, transform.position, Quaternion.identity);
            }
        }

    }

}
