using UnityEngine;

namespace Spelunky {

    /// <summary>
    /// Base class for entities that have physics-based movement (gravity, velocity, bouncing).
    /// Extends Entity and provides common physics behavior used by Bomb, Treasure, Block, etc.
    /// Subclasses can override behavior by:
    /// - Setting configuration fields (bounciness, friction, etc.)
    /// - Overriding virtual methods for custom physics behavior
    /// - Overriding OnPhysicsCollisionEnter for custom collision response
    /// </summary>
    public class PhysicsBody : Entity {

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

        public override void Awake() {
            base.Awake();
            audioSource = GetComponent<AudioSource>();
            Physics.OnCollisionEnterEvent.AddListener(OnPhysicsCollisionEnter);
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

        /// <summary>
        /// Play the bounce sound effect. Override to customize sound behavior.
        /// </summary>
        protected virtual void PlayBounceSound() {
            if (audioSource != null && bounceClip != null) {
                audioSource.PlayOneShot(bounceClip);
            }
        }

    }

}
