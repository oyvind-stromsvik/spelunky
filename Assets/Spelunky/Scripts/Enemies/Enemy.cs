using Gizmos = Popcron.Gizmos;
using UnityEngine;

namespace Spelunky {

    /// <summary>
    /// Base class for all enemies. Provides state machine support, velocity management,
    /// target tracking, and common enemy properties.
    /// </summary>
    [RequireComponent(typeof(EntityPhysics), typeof(EntityHealth), typeof(EntityVisuals))]
    public class Enemy : MonoBehaviour, ICrushable, IImpulseReceiver {

        public EntityPhysics Physics { get; private set; }
        public EntityHealth Health { get; private set; }
        public EntityVisuals Visuals { get; private set; }

        [Header("Enemy Settings")]
        public float moveSpeed = 32f;
        public int damage = 1;
        public Vector2 contactKnockback = new Vector2(256f, 512f);
        public LayerMask playerOverlapMask;

        [Header("Target Detection")]
        public LayerMask targetDetectionMask;
        public float detectionRange = 128f;
        public Vector2Int detectionBox = new Vector2Int(128, 64);
        public Vector2Int detectionOffset = new Vector2Int(0, -32);

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

        protected virtual void Awake() {
            Physics = GetComponent<EntityPhysics>();
            Health = GetComponent<EntityHealth>();
            Visuals = GetComponent<EntityVisuals>();
            stateMachine = new StateMachine();
            Physics.OnCollisionEnterEvent.AddListener(OnCollisionEnter);

            if (playerOverlapMask.value != 0) {
                Physics.blockingMask &= ~playerOverlapMask;
            }
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

        public void NotifyContactWithPlayer(Player player) {
            (stateMachine.CurrentState as EnemyState)?.OnContactWithPlayer(player);
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
            RaycastHit2D hit = Physics2D.Raycast(transform.position + offsetForward, Vector2.down, 2, Physics.blockingMask);
            Debug.DrawRay(transform.position + offsetForward, Vector2.down * 2, Color.green);
            return Physics.collisionInfo.down && hit.collider == null;
        }

        /// <summary>
        /// Try to detect a target within range using a raycast in the specified direction.
        /// </summary>
        public Transform DetectTargetInDirection(Vector2 position, Vector2 direction) {
            RaycastHit2D hit = Physics2D.Raycast(position, direction, detectionRange, targetDetectionMask);
            Debug.DrawRay(position, direction * detectionRange, Color.green);
            return hit.collider != null ? hit.transform : null;
        }

        /// <summary>
        /// Try to detect a target within a radius.
        /// </summary>
        public Transform DetectTargetInRadius(Vector2 position, float radius) {
            Collider2D hit = Physics2D.OverlapCircle(position, radius, targetDetectionMask);
            Gizmos.Circle(position, radius, Camera.main, Color.green);
            return hit != null ? hit.transform : null;
        }

        /// <summary>
        /// Try to detect a target within a box area.
        /// </summary>
        /// <param name="position">Position of the box in world space.</param>
        /// <param name="size">Size of the box (width, height).</param>
        /// <param name="angle">Rotation angle of the box in degrees.</param>
        /// <returns>The transform of the detected target, or null if none found.</returns>
        public Transform DetectTargetInBox(Vector2 position, Vector2 size, float angle = 0f) {
            Collider2D hit = Physics2D.OverlapBox(position, size, angle, targetDetectionMask);
            Gizmos.Square(position, size, Color.green);
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

        public bool IsCrushable => true;

        public void Crush() {
            Health.TakeDamage(int.MaxValue);
        }

        public void ApplyImpulse(Vector2 impulse) {
            velocity += impulse;
        }

    }

}
