using UnityEngine;

namespace Spelunky {

    [RequireComponent(typeof(Collider2D))]
    public class DamageTrigger : MonoBehaviour {
        public int damage;

        private Collider2D _collider;

        private void Awake() {
            _collider = GetComponent<Collider2D>();
        }

        private void OnTriggerEnter2D(Collider2D other) {
            EntityHealth health = other.GetComponent<EntityHealth>();

            if (health != null) {
                health.TakeDamage(damage);
            }
        }
    }

}