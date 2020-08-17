using UnityEngine;

namespace Spelunky {

    [RequireComponent(typeof(PhysicsObject), typeof(EntityHealth), typeof(EntityVisuals))]
    public class Enemy : MonoBehaviour, IObjectController {

        public GameObject bloodParticles;

        public PhysicsObject PhysicsObject { get; private set; }
        public EntityHealth EntityHealth { get; private set; }
        public EntityVisuals EntityVisuals { get; private set; }

        public virtual void Awake() {
            PhysicsObject = GetComponent<PhysicsObject>();
            EntityHealth = GetComponent<EntityHealth>();
            EntityVisuals = GetComponent<EntityVisuals>();

            EntityHealth.HealthChanged.AddListener(OnHealthChanged);
        }

        public virtual bool IgnoreCollider(Collider2D collider, CollisionDirection direction) {
            return false;
        }

        public void OnCollision(CollisionInfo collisionInfo) {
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

        public void UpdateVelocity(ref Vector2 velocity) {

        }
    }
}
