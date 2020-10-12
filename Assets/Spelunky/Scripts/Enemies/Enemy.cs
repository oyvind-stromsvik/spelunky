using UnityEngine;

namespace Spelunky {

    [RequireComponent(typeof(EntityPhysics), typeof(EntityHealth), typeof(EntityVisuals))]
    public class Enemy : MonoBehaviour {
        public GameObject bloodParticles;

        public EntityPhysics EntityPhysics { get; private set; }
        public EntityHealth EntityHealth { get; private set; }
        public EntityVisuals EntityVisuals { get; private set; }

        public virtual void Awake() {
            EntityPhysics = GetComponent<EntityPhysics>();
            EntityHealth = GetComponent<EntityHealth>();
            EntityVisuals = GetComponent<EntityVisuals>();

            EntityHealth.HealthChangedEvent.AddListener(OnHealthChanged);
        }

        public virtual void OnHealthChanged() {
            if (EntityHealth.CurrentHealth <= 0) {
                Die();
            }
        }

        private void Die() {
            Instantiate(bloodParticles, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }

}