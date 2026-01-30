using UnityEngine;

namespace Spelunky {

    /// <summary>
    /// Base class for objects that have physics-based movement (gravity, velocity, bouncing).
    /// Provides common physics behavior used by Bomb, Treasure, Block, etc.
    /// Subclasses can override behavior by:
    /// - Setting configuration fields (bounciness, friction, etc.)
    /// - Overriding virtual methods for custom physics behavior
    /// - Overriding OnPhysicsCollisionEnter for custom collision response
    /// </summary>
    [RequireComponent(typeof(EntityPhysics))]
    public class PhysicsBody : MonoBehaviour, IImpulseReceiver {

        public EntityPhysics Physics { get; private set; }
        public EntityVisuals Visuals { get; private set; }

        [Header("Physics Settings")]
        [Tooltip("Multiplier applied to velocity on bounce (0.5 = lose half velocity)")]
        public float bounceDampening = 0.5f;

        [Tooltip("Friction applied when grounded (0.9 = retain 90% velocity)")]
        public float groundFriction = 0.9f;

        [Tooltip("Whether this body bounces off surfaces")]
        public bool bounces = true;

        [Tooltip("Whether to stop vertical velocity when grounded (vs continuous gravity)")]
        public bool stopOnGround = true;

        [Tooltip("Whether to apply friction when grounded")]
        public bool applyFriction = true;

        [Header("Impact Damage")]
        [Tooltip("Whether this body deals damage on impact.")]
        public bool dealsImpactDamage = true;

        [Tooltip("Velocity threshold required to deal impact damage.")]
        public float impactDamageThreshold = 256f;

        [Tooltip("Damage dealt on impact when above the threshold.")]
        public int impactDamage = 1;

        [Header("Audio")]
        public AudioClip bounceClip;

        [Tooltip("Minimum velocity magnitude to trigger bounce sound")]
        public float bounceSoundThreshold = 100f;

        protected Vector2 velocity;
        protected AudioSource audioSource;

        /// <summary>
        /// Public access to current velocity.
        /// </summary>
        public Vector2 Velocity {
            get { return velocity; }
            set { velocity = value; }
        }

        protected virtual void Awake() {
            Physics = GetComponent<EntityPhysics>();
            Visuals = GetComponent<EntityVisuals>();
            audioSource = GetComponent<AudioSource>();
            Physics.OnCollisionEnterEvent.AddListener(OnPhysicsCollisionEnter);
            Physics.OnOverlapEnterEvent.AddListener(OnPhysicsOverlapEnter);
        }

        protected virtual void Update() {
            ApplyGravity();
            ApplyFriction();
            Move();
        }

        /// <summary>
        /// Apply gravity to velocity. Override for custom gravity behavior.
        /// </summary>
        protected virtual void ApplyGravity() {
            if (stopOnGround && Physics.collisionInfo.down) {
                velocity.y = 0;
            }
            else {
                velocity.y += PhysicsManager.gravity.y * Time.deltaTime;
            }
        }

        /// <summary>
        /// Apply ground friction. Override for custom friction behavior.
        /// </summary>
        protected virtual void ApplyFriction() {
            if (applyFriction && Physics.collisionInfo.down) {
                velocity *= groundFriction;
            }
        }

        /// <summary>
        /// Move the physics body. Override for custom movement behavior.
        /// </summary>
        protected virtual void Move() {
            Physics.Move(velocity * Time.deltaTime);
        }

        /// <summary>
        /// Handle physics collision. Default behavior is to bounce off surfaces.
        /// Override for custom collision response.
        /// </summary>
        protected virtual void OnPhysicsCollisionEnter(CollisionInfo collisionInfo) {
            HandleImpactDamage(collisionInfo);

            if (!bounces) {
                return;
            }

            bool shouldPlaySound = false;

            if (collisionInfo.left || collisionInfo.right) {
                if (Mathf.Abs(velocity.x) > bounceSoundThreshold) {
                    shouldPlaySound = true;
                }

                velocity.x *= -1f;
            }

            if (collisionInfo.up || collisionInfo.down) {
                if (Mathf.Abs(velocity.y) > bounceSoundThreshold) {
                    shouldPlaySound = true;
                }

                velocity.y *= -1f;
            }

            velocity *= bounceDampening;

            if (shouldPlaySound) {
                PlayBounceSound();
            }
        }

        protected void HandleImpactDamage(CollisionInfo collisionInfo) {
            if (!dealsImpactDamage || impactDamage <= 0) {
                return;
            }

            IDamageable damageableHorizontal = null;
            IDamageable damageableVertical = null;

            if (collisionInfo.left && velocity.x <= -impactDamageThreshold || collisionInfo.right && velocity.x >= impactDamageThreshold) {
                damageableHorizontal = GetDamageable(collisionInfo.colliderHorizontal);
            }

            if (collisionInfo.down && velocity.y <= -impactDamageThreshold || collisionInfo.up && velocity.y >= impactDamageThreshold) {
                damageableVertical = GetDamageable(collisionInfo.colliderVertical);
            }

            if (damageableHorizontal != null) {
                damageableHorizontal.TryTakeDamage(impactDamage);
            }

            if (damageableVertical != null && !ReferenceEquals(damageableVertical, damageableHorizontal)) {
                damageableVertical.TryTakeDamage(impactDamage);
            }
        }

        protected void HandleOverlapImpactDamage(Collider2D collider2D) {
            if (!dealsImpactDamage || impactDamage <= 0) {
                return;
            }

            if (collider2D == null) {
                return;
            }

            if (IsLayerInMask(collider2D.gameObject.layer, Physics.blockingMask)) {
                return;
            }

            if (velocity.magnitude < impactDamageThreshold) {
                return;
            }

            IDamageable damageable = GetDamageable(collider2D);
            if (damageable != null) {
                damageable.TryTakeDamage(impactDamage);
            }
        }

        private static IDamageable GetDamageable(Collider2D collider2D) {
            if (collider2D == null) {
                return null;
            }

            return collider2D.GetComponentInParent<IDamageable>();
        }

        private static bool IsLayerInMask(int layer, LayerMask mask) {
            return (mask.value & (1 << layer)) != 0;
        }

        /// <summary>
        /// Play the bounce sound effect. Override to customize sound behavior.
        /// </summary>
        protected virtual void PlayBounceSound() {
            if (audioSource != null && bounceClip != null) {
                audioSource.PlayOneShot(bounceClip);
            }
        }

        private void OnPhysicsOverlapEnter(Collider2D collider2D) {
            HandleOverlapImpactDamage(collider2D);
        }

        public void ApplyImpulse(Vector2 impulse) {
            velocity += impulse;
        }

    }

}
