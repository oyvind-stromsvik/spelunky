using Gizmos = Popcron.Gizmos;
using System.Collections.Generic;
using UnityEngine;

namespace Spelunky {

    [RequireComponent(typeof(EntityPhysics), typeof(EntityHealth), typeof(EntityVisuals))]
    public class Player : MonoBehaviour, ICrushable, IImpulseReceiver, ILateTickable {

        public EntityPhysics Physics { get; private set; }
        public EntityHealth Health { get; private set; }
        public EntityVisuals Visuals { get; private set; }

        public PlayerInput Input { get; private set; }
        public PlayerAudio Audio { get; private set; }
        public PlayerInventory Inventory { get; private set; }
        public PlayerAccessories Accessories { get; private set; }
        public PlayerHolding Holding { get; private set; }
        
        [Header("Whip")]
        public SpriteAnimation attackWithWhipAnimation;
        public Vector2Int whipDamageArea = new Vector2Int(42, 20);
        public Vector2Int whipOffset = new Vector2Int(-3, 10);
        public int whipDamage = 1;

        [Header("States")]
        public PlayerGroundedState groundedState;
        public PlayerInAirState inAirState;
        public PlayerHangingState hangingState;
        public PlayerClimbingState climbingState;
        public PlayerCrawlToHangState crawlToHangState;
        public PlayerEnterDoorState enterDoorState;
        public PlayerSplatState splatState;

        [Header("Crap")]
        public LayerMask edgeGrabLayerMask;
        public CameraFollow cam;
        public Exit _exitDoor;

        [Header("Abilities")]
        public Bomb bomb;
        public Rope rope;
        public SpriteAnimation throwAnimation;

        [Header("Throwing")]
        [Tooltip("The speed at which items are thrown.")]
        public float throwItemSpeed = 286f;
        [Tooltip("The speed at which items are placed when crouching.")]
        public float placeItemSpeed = 64f;

        [Header("Combat")]
        public LayerMask enemyOverlapMask;
        public int stompDamage = 1;
        public float stompTopTolerance = 1f;
        public float knockbackDuration = 0.2f;

        [Header("Movement")]
        public float maxJumpHeight;
        public float minJumpHeight;
        public float timeToJumpApex;
        public float accelerationTime;
        public float climbSpeed;
        public float crawlSpeed;
        public float runSpeed;
        public float sprintSpeed;
        public float pushBlockSpeed;

        [HideInInspector] public bool sprinting;

        [Tooltip("The time in seconds that we are considered grounded after leaving a platform. Allows us to easier time jumps.")]
        public float groundedGracePeriod = 0.1f;

        [HideInInspector] public float groundedGraceTimer;

        private float _gravity;
        private float _maxJumpVelocity;
        private float _minJumpVelocity;

        public float MinJumpVelocity => _minJumpVelocity;

        // TODO: Make this private. Currently the jump logic in State.cs is the only place we set this, but I'm not
        // entirely sure how to refactor that so that.
        // This is the velocity we want to apply to the player this frame.
        [HideInInspector] public Vector2 requestedVelocity;
        private float _velocityXSmoothing;
        [HideInInspector] public Vector2 directionalInput;
        private float _speed;

        [HideInInspector] public bool recentlyJumped;
        [HideInInspector] public float _lastJumpTimer;

        [HideInInspector] public float _lookTimer;
        [HideInInspector] public float _timeBeforeLook = 1f;

        public StateMachine stateMachine = new StateMachine();

        private readonly Collider2D[] _enemyOverlapResults = new Collider2D[8];
        private ContactFilter2D _enemyOverlapFilter;
        private float _knockbackTimer;
        private Timer _fallThroughTimer;

        // Attack state
        private bool _isAttacking;
        private float _attackTimer;
        private float _attackDuration;
        private HashSet<Enemy> _hitEnemies;

        /// <summary>
        /// Helper property to access the current state as a player-specific State.
        /// Use this when calling player-specific methods like UpdateState(), ChangePlayerVelocity(), etc.
        /// </summary>
        public PlayerState CurrentPlayerState {
            get { return (PlayerState)stateMachine.CurrentState; }
        }

        protected virtual void Awake() {
            Physics = GetComponent<EntityPhysics>();
            Health = GetComponent<EntityHealth>();
            Visuals = GetComponent<EntityVisuals>();

            _gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
            _maxJumpVelocity = CalculateJumpVelocityForHeight(maxJumpHeight);
            _minJumpVelocity = CalculateJumpVelocityForHeight(minJumpHeight);

            Input = GetComponent<PlayerInput>();
            Audio = GetComponent<PlayerAudio>();
            Inventory = GetComponent<PlayerInventory>();
            Accessories = GetComponent<PlayerAccessories>();
            Holding = GetComponent<PlayerHolding>();

            Health.HealthChangedEvent.AddListener(OnHealthChanged);

            SetupEnemyOverlapFilter();

            if (enemyOverlapMask.value != 0) {
                Physics.blockingMask &= ~enemyOverlapMask;
            }
        }

        private void Start() {
            stateMachine.AttemptToChangeState(groundedState);
        }

        protected virtual void OnEnable() {
            EntityManager.Instance?.Register(this);
        }

        protected virtual void OnDisable() {
            EntityManager.Instance?.Unregister(this);
        }

        // ITickable implementation
        public bool IsTickActive => true;

        public void Tick() {
            if (_isAttacking) {
                Gizmos.Square(
                    (Vector2)transform.position + new Vector2Int(whipOffset.x * Visuals.facingDirection, whipOffset.y),
                    whipDamageArea,
                    Color.yellow
                );
                UpdateAttack();
            }

            CurrentPlayerState.UpdateState();

            SetPlayerSpeed();
            CalculateVelocity();

            // Used for giving ourselves a small amount of time before we grab
            // a ladder again. Can probably be solved cleaner. Without this at
            // the moment we would be unable to jump up or down ladders if we
            // were holding up or down at the same time.
            if (recentlyJumped) {
                _lastJumpTimer += Time.deltaTime;
                if (_lastJumpTimer > 0.35f) {
                    _lastJumpTimer = 0;
                    recentlyJumped = false;
                }
            }

            Physics.Move(requestedVelocity * Time.deltaTime);
        }

        // ILateTickable implementation
        public void LateTick() {
            HandleEnemyOverlaps();

            CurrentPlayerState.ChangePlayerVelocityAfterMove(ref requestedVelocity);
        }

        private void SetPlayerSpeed() {
            if (directionalInput.x != 0) {
                if (directionalInput.y < 0) {
                    _speed = crawlSpeed;
                }
                else if (sprinting) {
                    _speed = sprintSpeed;
                }
                else {
                    _speed = runSpeed;
                }
            }
        }

        private void CalculateVelocity() {
            if (_knockbackTimer > 0f) {
                _knockbackTimer -= Time.deltaTime;
                requestedVelocity.y += _gravity * Time.deltaTime;
                return;
            }

            float targetVelocityX = directionalInput.x * _speed;
            // TODO: This means we have a horizontal velocity for many seconds after letting go of the input. This tiny
            // velocity apparently can cause us to get dragged after enemies. It's of course the collision detection
            // that needs to be fixed and not the fact that we have deceleration on our movement, but I don't think I
            // understand what's going on here so I should try to understand it. Currently it doesn't really do what I
            // want. I don't want us to have a lingering velocity for many seconds after we stop moving. I want this to
            // just simulate some slight acceleration and deceleration, maybe over a second or something? And then we
            // also need to be able to affect this when we introduce ice which should be slippery.
            requestedVelocity.x = Mathf.SmoothDamp(requestedVelocity.x, targetVelocityX, ref _velocityXSmoothing, accelerationTime);

            requestedVelocity.y += _gravity * Time.deltaTime;

            CurrentPlayerState.ChangePlayerVelocity(ref requestedVelocity);
        }

        public void ThrowBomb() {
            if (Inventory.numberOfBombs <= 0) {
                return;
            }

            Inventory.UseBomb();

            Bomb bombInstance = Instantiate(bomb, Holding.holdPosition.position, Quaternion.identity);
            Vector2 throwVelocity = CalculateThrowVelocity();
            bool affectedByGravity = !Accessories.HasPitchersMitt;

            bombInstance.OnThrown(this, throwVelocity, affectedByGravity);
            
            Visuals.animator.PlayOnceUninterrupted(throwAnimation);
        }

        public Vector2 CalculateThrowVelocity(float itemVelocityMultiplier = 1f) {
            // Throw angles in degrees (0 = horizontal, positive = upward).
            const float upwardThrowAngle = 60f;
            const float normalThrowAngle = 25f;
            const float downwardThrowAngle = -60f;
            const float horizontalAngle = 0f;

            float angle;
            float speed;

            // Holding up: upward throw (same with or without PitchersMitt).
            if (directionalInput.y > 0) {
                angle = upwardThrowAngle;
                speed = throwItemSpeed;
            }
            // PitchersMitt makes throws perfectly horizontal when not holding up.
            else if (Accessories.HasPitchersMitt) {
                angle = horizontalAngle;
                speed = throwItemSpeed;
            }
            // Holding down while grounded: place item gently.
            else if (directionalInput.y < 0 && Physics.collisionInfo.down) {
                angle = normalThrowAngle;
                speed = placeItemSpeed;
            }
            // Holding down while in air: downward throw.
            else if (directionalInput.y < 0) {
                angle = downwardThrowAngle;
                speed = throwItemSpeed;
            }
            // Normal throw: slight upward arc.
            else {
                angle = normalThrowAngle;
                speed = throwItemSpeed;
            }

            // Convert angle to direction vector.
            float radians = angle * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(radians) * Visuals.facingDirection, Mathf.Sin(radians));

            // Calculate base throw velocity.
            Vector2 throwVelocity = direction * speed * itemVelocityMultiplier;

            // Add player's velocity to throw for momentum transfer.
            throwVelocity += requestedVelocity;

            return throwVelocity;
        }

        /// <summary>
        /// Calculates the initial velocity needed to reach a specific jump height.
        /// Uses the kinematic equation: v = sqrt(2 * g * h)
        /// </summary>
        public float CalculateJumpVelocityForHeight(float height) {
            return Mathf.Sqrt(2 * Mathf.Abs(_gravity) * height);
        }

        /// <summary>
        /// Gets the current maximum jump height including any bonuses from accessories.
        /// </summary>
        public float GetCurrentMaxJumpHeight() {
            return maxJumpHeight + Accessories.JumpHeightBonus;
        }

        public void ThrowRope() {
            if (Inventory.numberOfRopes <= 0) {
                return;
            }

            Inventory.UseRope();

            Rope ropeInstance = Instantiate(rope, transform.position, Quaternion.identity);
            if (ReferenceEquals(stateMachine.CurrentState, groundedState) && directionalInput.y < 0) {
                ropeInstance.placePosition = transform.position + Visuals.facingDirection * Vector3.right * Tile.Width;
            }
            else {
                Visuals.animator.PlayOnceUninterrupted(throwAnimation);
            }
        }

        public void Use() {
            if (_exitDoor == null) {
                return;
            }

            stateMachine.AttemptToChangeState(enterDoorState);
        }

        public void Attack() {
            if (_isAttacking) {
                return;
            }

            BeginAttack();
        }

        private void BeginAttack() {
            _isAttacking = true;
            _attackTimer = 0f;
            _attackDuration = Visuals.animator.GetAnimationLength(attackWithWhipAnimation);
            _hitEnemies = new HashSet<Enemy>();

            Visuals.animator.PlayOnceUninterrupted(attackWithWhipAnimation);
            Audio.Play(Audio.whipClip, 0.7f);
        }

        private void UpdateAttack() {
            // Damage enemies in whip area.
            int hitCount = Physics2D.OverlapBox(
                (Vector2)transform.position + new Vector2Int(whipOffset.x * Visuals.facingDirection, whipOffset.y),
                whipDamageArea,
                0f,
                _enemyOverlapFilter,
                _enemyOverlapResults
            );

            for (int i = 0; i < hitCount; i++) {
                Collider2D hit = _enemyOverlapResults[i];
                if (hit == null || hit == Physics.Collider) {
                    continue;
                }

                Enemy enemy = hit.GetComponentInParent<Enemy>();
                if (enemy == null || _hitEnemies.Contains(enemy)) {
                    continue;
                }

                enemy.Health.TakeDamage(whipDamage);
                _hitEnemies.Add(enemy);
            }

            _attackTimer += Time.deltaTime;
            if (_attackTimer >= _attackDuration) {
                EndAttack();
            }
        }

        private void EndAttack() {
            _isAttacking = false;
            _hitEnemies = null;
        }

        public void EnteredDoorway(Exit door) {
            _exitDoor = door;
        }

        public void ExitedDoorway(Exit door) {
            _exitDoor = null;
        }

        public void Splat() {
            stateMachine.AttemptToChangeState(splatState);
        }

        public bool IsCrushable => true;

        public void Crush() {
            if (!ReferenceEquals(stateMachine.CurrentState, splatState)) {
                Splat();
            }
        }

        private void OnHealthChanged() {
            if (Health.CurrentHealth <= 0) {
                stateMachine.AttemptToChangeState(splatState);
            }
        }

        private void HandleEnemyOverlaps() {
            if (enemyOverlapMask.value == 0) {
                return;
            }

            Vector2 overlapSize = GetEnemyOverlapSize();
            Vector2 overlapPosition = (Vector2)transform.position + Physics.Collider.offset;

            int hitCount = Physics2D.OverlapBox(
                overlapPosition,
                overlapSize,
                0f,
                _enemyOverlapFilter,
                _enemyOverlapResults
            );

            if (hitCount == 0) {
                return;
            }

            for (int i = 0; i < hitCount; i++) {
                Collider2D hit = _enemyOverlapResults[i];
                if (hit == null || hit == Physics.Collider) {
                    continue;
                }

                Enemy enemy = hit.GetComponentInParent<Enemy>();
                if (enemy == null) {
                    continue;
                }

                if (TryStompEnemy(enemy, hit)) {
                    break;
                }

                if (Health.isInvulnerable) {
                    break;
                }

                ApplyContactDamage(enemy);
                break;
            }
        }

        private Vector2 GetEnemyOverlapSize() {
            Vector2 size = Physics.Collider.size - Vector2.one * 0.5f;
            return new Vector2(Mathf.Max(1f, size.x), Mathf.Max(1f, size.y));
        }

        private bool TryStompEnemy(Enemy enemy, Collider2D enemyCollider) {
            float playerBottom = Physics.Collider.bounds.min.y;
            float enemyTop = enemyCollider.bounds.max.y;
            if (playerBottom < enemyTop - stompTopTolerance) {
                return false;
            }

            EntityHealth enemyHealth = enemy.Health;
            if (enemyHealth == null) {
                return false;
            }

            enemyHealth.TakeDamage(stompDamage);
            BounceOffEnemy();
            return true;
        }

        private void BounceOffEnemy() {
            requestedVelocity.y = _maxJumpVelocity;
            if (!ReferenceEquals(stateMachine.CurrentState, inAirState)) {
                stateMachine.AttemptToChangeState(inAirState);
            }
        }

        private void ApplyContactDamage(Enemy enemy) {
            float direction = Mathf.Sign(transform.position.x - enemy.transform.position.x);
            if (Mathf.Approximately(direction, 0f)) {
                direction = Visuals.facingDirection;
            }

            Vector2 knockback = new Vector2(Mathf.Abs(enemy.contactKnockback.x) * direction, enemy.contactKnockback.y);
            ApplyKnockback(knockback);

            enemy.NotifyContactWithPlayer(this);
            Health.TakeDamage(enemy.damage);
        }

        private void ApplyKnockback(Vector2 knockback) {
            requestedVelocity = knockback;
            _knockbackTimer = knockbackDuration;

            if (!ReferenceEquals(stateMachine.CurrentState, inAirState)) {
                stateMachine.AttemptToChangeState(inAirState);
            }
        }

        public void ApplyImpulse(Vector2 impulse) {
            ApplyKnockback(impulse);
        }

        private void SetupEnemyOverlapFilter() {
            _enemyOverlapFilter = new ContactFilter2D {
                useLayerMask = true,
                layerMask = enemyOverlapMask,
                useTriggers = false
            };
        }

        private void OnValidate() {
            SetupEnemyOverlapFilter();
        }

        public void BeginFallThroughPlatformWindow(float duration) {
            _fallThroughTimer?.Cancel();

            Physics.collisionInfo.fallingThroughPlatform = true;
            _fallThroughTimer = TimerManager.Instance.CreateTimer(duration, () => {
                Physics.collisionInfo.fallingThroughPlatform = false;
                _fallThroughTimer = null;
            });
        }

        // TODO: Make it so that we can show debug info for whatever entity we select.
        private void OnGUI() {
            string[] debugInfo = {
                "--- Player info ---",
                "State: " + stateMachine.CurrentState.GetType().Name,
                "--- Physics info --- ",
                "Requested Velocity X: " + requestedVelocity.x,
                "Requested Velocity Y: " + requestedVelocity.y,
                "Velocity X: " + Physics.Velocity.x,
                "Velocity Y: " + Physics.Velocity.y,
                "--- Physics Collision info --- ",
                "Down: " + Physics.collisionInfo.down,
                "Left: " + Physics.collisionInfo.left,
                "Right: " + Physics.collisionInfo.right,
                "Up: " + Physics.collisionInfo.up,
                "Collider horizontal: " + Physics.collisionInfo.colliderHorizontal,
                "Collider vertical: " + Physics.collisionInfo.colliderVertical,
                "Falling through platform: " + Physics.collisionInfo.fallingThroughPlatform
            };
            for (int i = 0; i < debugInfo.Length; i++) {
                GUI.Label(new Rect(8, 100 + 16 * i, 300, 22), debugInfo[i]);
            }
        }

    }

}
