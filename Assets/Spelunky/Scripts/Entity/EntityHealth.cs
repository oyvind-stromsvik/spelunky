using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Spelunky {

    public class EntityHealth : MonoBehaviour, IDamageable {

        public GameObject bloodParticles;

        public UnityEvent HealthChangedEvent { get; private set; } = new UnityEvent();

        public int maxHealth;
        public int CurrentHealth { get; private set; }

        [Header("Invulnerability")]
        public SpriteRenderer spriteRenderer;
        public float invulnerabilityDuration;
        public int numberOfInvulnerabilityFlashes = 10;
        public Color invulnerabilityFlashColor;
        // TODO: We probably want to differentiate between being invulnerable and recently being damage and
        // "not touchable".
        public bool isInvulnerable;

        private void Awake() {
            SetHealth(maxHealth);
        }

        public void TakeDamage(int damage) {
            if (isInvulnerable) {
                return;
            }
            
            Instantiate(bloodParticles, transform.position, Quaternion.identity);

            CurrentHealth -= damage;
            if (CurrentHealth < 0) {
                CurrentHealth = 0;
            }

            HealthChangedEvent?.Invoke();
            
            if (CurrentHealth <= 0) {
                Die();
            }
            else {
                if (invulnerabilityDuration > 0f) {
                    StartCoroutine(InvulnerabilityTime());
                }
            }
        }

        public bool TryTakeDamage(int damage) {
            if (isInvulnerable) {
                return false;
            }

            TakeDamage(damage);
            return true;
        }

        private void SetHealth(int value) {
            CurrentHealth = value;
            HealthChangedEvent?.Invoke();
        }

        private void Die() {
            Destroy(gameObject);
        }

        /// <summary>
        /// The time we're invulnerable after being hit.
        /// </summary>
        /// <returns></returns>
        private IEnumerator InvulnerabilityTime() {
            isInvulnerable = true;

            Color originalColor = spriteRenderer != null ? spriteRenderer.color : Color.white;
            int flashCount = Mathf.Max(1, numberOfInvulnerabilityFlashes);
            float startFlashInterval = invulnerabilityDuration / (flashCount * 0.5f); // Start slower
            float endFlashInterval = invulnerabilityDuration / (flashCount * 2f);    // End faster
            float elapsed = 0f;

            while (elapsed < invulnerabilityDuration) {
                float t = elapsed / invulnerabilityDuration;
                float flashInterval = Mathf.Lerp(startFlashInterval, endFlashInterval, t);

                if (spriteRenderer != null) {
                    spriteRenderer.color = invulnerabilityFlashColor;
                    yield return new WaitForSeconds(flashInterval / 2);
                    spriteRenderer.color = originalColor;
                    yield return new WaitForSeconds(flashInterval / 2);
                } else {
                    yield return new WaitForSeconds(flashInterval);
                }

                elapsed += flashInterval;
            }

            isInvulnerable = false;
        }
    }

}
