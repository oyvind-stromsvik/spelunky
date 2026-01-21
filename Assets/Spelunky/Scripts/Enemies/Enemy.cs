using UnityEngine;

namespace Spelunky {

    /// <summary>
    /// Base class for all enemies. Provides state machine support, velocity management,
    /// target tracking, and common enemy properties.
    /// </summary>
    public class Enemy : Entity {

        [Header("Enemy Settings")]
        public float moveSpeed = 32f;
        public int damage = 1;

        [Header("Target Detection")]
        public LayerMask targetDetectionMask;
        public float detectionRange = 128f;

        [Header("State Machine")]
        [Tooltip("The initial state to enter on Start")]
        public EnemyState initialState;

        /// <summary>
        /// The enemy's state machine for AI behavior.
        /// </summary>
        public StateMachine stateMachine { get; private set; }

        /// <summary>
        /// Current movement velocity.
        /// </summary>
        public Vector2 velocity;

        /// <summary>
        /// The target this enemy is tracking/chasing (usually the player).
        /// </summary>
        public Transform target { get; set; }

        /// <summary>
        /// Whether this enemy has been activated/triggered.
        /// </summary>
        public bool isActivated { get; set; }

        public override void Awake() {
            base.Awake();
            stateMachine = new StateMachine();
            Physics.OnCollisionEnterEvent.AddListener(OnCollisionEnter);
        }

        protected virtual void Start() {
            if (initialState != null) {
                stateMachine.AttemptToChangeState(initialState);
            }
        }

        protected virtual void Update() {
            (stateMachine.CurrentState as EnemyState)?.UpdateState();
        }

        private void OnCollisionEnter(CollisionInfo collisionInfo) {
            (stateMachine.CurrentState as EnemyState)?.OnCollisionEnter(collisionInfo);
        }

        protected virtual void OnTriggerEnter2D(Collider2D other) {
            (stateMachine.CurrentState as EnemyState)?.OnTriggerEnter(other);
        }

        /// <summary>
        /// Apply gravity to velocity. Call this in states that need gravity.
        /// </summary>
        public void ApplyGravity() {
            if (Physics.collisionInfo.down) {
                velocity.y = 0;
            }
            else {
                velocity.y += PhysicsManager.gravity.y * Time.deltaTime;
            }
        }

        /// <summary>
        /// Move the enemy using current velocity.
        /// </summary>
        public void Move() {
            Physics.Move(velocity * Time.deltaTime);
        }

        /// <summary>
        /// Face toward the current target.
        /// </summary>
        public void FaceTarget() {
            if (target == null) {
                return;
            }

            float direction = Mathf.Sign(target.position.x - transform.position.x);
            if (direction > 0 && !Visuals.isFacingRight) {
                Visuals.FlipCharacter();
            }
            else if (direction < 0 && Visuals.isFacingRight) {
                Visuals.FlipCharacter();
            }
        }

        /// <summary>
        /// Face in the direction of movement.
        /// </summary>
        public void FaceMovementDirection() {
            if (velocity.x > 0 && !Visuals.isFacingRight) {
                Visuals.FlipCharacter();
            }
            else if (velocity.x < 0 && Visuals.isFacingRight) {
                Visuals.FlipCharacter();
            }
        }

        /// <summary>
        /// Check if we're at a ledge (used for patrol behavior).
        /// </summary>
        public bool IsAtLedge() {
            Vector3 offsetForward = new Vector3(Physics.Collider.size.x * Visuals.facingDirection / 2f, 1, 0);
            RaycastHit2D hit = Physics2D.Raycast(transform.position + offsetForward, Vector2.down, 2, Physics.collisionMask);
            Debug.DrawRay(transform.position + offsetForward, Vector2.down * 2, Color.green);
            return Physics.collisionInfo.down && hit.collider == null;
        }

        /// <summary>
        /// Try to detect a target within range using a raycast in the specified direction.
        /// </summary>
        public Transform DetectTargetInDirection(Vector2 direction) {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, detectionRange, targetDetectionMask);
            Debug.DrawRay(transform.position, direction * detectionRange, Color.green);
            return hit.collider != null ? hit.transform : null;
        }

        /// <summary>
        /// Try to detect a target within a radius.
        /// </summary>
        public Transform DetectTargetInRadius(float radius) {
            Collider2D hit = Physics2D.OverlapCircle(transform.position, radius, targetDetectionMask);
            return hit != null ? hit.transform : null;
        }

        /// <summary>
        /// Deal damage to a player.
        /// </summary>
        public void DealDamage(Player player, Vector2 knockback) {
            if (player == null) {
                return;
            }

            player.velocity = knockback;
            player.Health.TakeDamage(damage);
        }

    }

}
