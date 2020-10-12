using UnityEngine;
using UnityEngine.Events;

namespace Spelunky {

    public class EntityHealth : MonoBehaviour {
        public UnityEvent HealthChangedEvent { get; private set; } = new UnityEvent();

        public float invulnerabilityDuration;
        private float _invulnerabilityTimer;

        public int maxHealth;
        public int CurrentHealth { get; private set; }

        public bool IsInvulernable {
            get { return _invulnerabilityTimer <= invulnerabilityDuration; }
        }

        private void Reset() {
            invulnerabilityDuration = 0f;
            maxHealth = 1;
        }

        private void Awake() {
            SetHealth(maxHealth);
        }

        private void Update() {
            _invulnerabilityTimer += Time.deltaTime;
        }

        public void TakeDamage(int damage) {
            if (_invulnerabilityTimer <= invulnerabilityDuration) {
                return;
            }

            CurrentHealth -= damage;
            if (CurrentHealth < 0) {
                CurrentHealth = 0;
            }

            HealthChangedEvent?.Invoke();

            _invulnerabilityTimer = 0f;
        }

        public void SetHealth(int value) {
            CurrentHealth = value;
            HealthChangedEvent?.Invoke();
        }
    }

}
