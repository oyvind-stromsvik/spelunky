using Gizmos = Popcron.Gizmos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spelunky {

    public class Player : Entity, ICrushable {

        public PlayerInput Input { get; private set; }
        public PlayerAudio Audio { get; private set; }
        public PlayerInventory Inventory { get; private set; }
        
        [Header("Whip")]
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
        [HideInInspector] public float _maxJumpVelocity;

        [HideInInspector] public float _minJumpVelocity;

        // TODO: Make this private. Currently the jump logic in State.cs is the only place we set this, but I'm not
        // entirely sure how to refactor that so that.
        [HideInInspector] public Vector2 velocity;
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
        private Coroutine _fallThroughCoroutine;

        /// <summary>
        /// Helper property to access the current state as a player-specific State.
        /// Use this when calling player-specific methods like UpdateState(), ChangePlayerVelocity(), etc.
        /// </summary>
        public PlayerState CurrentPlayerState {
            get { return (PlayerState)stateMachine.CurrentState; }
        }

        public override void Awake() {
            base.Awake();

            _gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
            _maxJumpVelocity = Mathf.Abs(_gravity) * timeToJumpApex;
            _minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(_gravity) * minJumpHeight);

            Input = GetComponent<PlayerInput>();
            Audio = GetComponent<PlayerAudio>();
            Inventory = GetComponent<PlayerInventory>();

            Health.HealthChangedEvent.AddListener(OnHealthChanged);

            SetupEnemyOverlapFilter();

            if (enemyOverlapMask.value != 0) {
                Physics.collisionMask &= ~enemyOverlapMask;
            }
        }

        private void Start() {
            stateMachine.AttemptToChangeState(groundedState);
        }

        private void Update() {
            if (_isAttacking) {
                Gizmos.Square(
                    (Vector2)transform.position + new Vector2Int(whipOffset.x * Visuals.facingDirection, whipOffset.y), 
                    whipDamageArea, 
                    Color.yellow
                );
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

            Physics.Move(velocity * Time.deltaTime);

            HandleEnemyOverlaps();

            CurrentPlayerState.ChangePlayerVelocityAfterMove(ref velocity);
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
                velocity.y += _gravity * Time.deltaTime;
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
            velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref _velocityXSmoothing, accelerationTime);

            velocity.y += _gravity * Time.deltaTime;

            CurrentPlayerState.ChangePlayerVelocity(ref velocity);
        }

        public void ThrowBomb() {
            if (Inventory.numberOfBombs <= 0) {
                return;
            }

            Inventory.UseBomb();

            Bomb bombInstance = Instantiate(bomb, transform.position + Vector3.up * 2f, Quaternion.identity);
            Vector2 bombVelocity = new Vector2(256 * Visuals.facingDirection, 128);
            if (directionalInput.y == 1) {
                bombVelocity = new Vector2(128 * Visuals.facingDirection, 256);
            }
            else if (directionalInput.y == -1) {
                if (Physics.collisionInfo.down) {
                    bombVelocity = new Vector2(64 * Visuals.facingDirection, 0);
                }
                else {
                    bombVelocity = new Vector2(128 * Visuals.facingDirection, -256);
                }
            }

            bombInstance.Velocity = bombVelocity;
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
        }

        public void Use() {
            if (_exitDoor == null) {
                return;
            }

            stateMachine.AttemptToChangeState(enterDoorState);
        }

        private bool _isAttacking;

        public void Attack() {
            if (_isAttacking) {
                return;
            }

            StartCoroutine(DoAttack());
        }

        private IEnumerator DoAttack() {
            _isAttacking = true;

            Visuals.animator.PlayOnceUninterrupted("AttackWithWhip");
            Audio.Play(Audio.whipClip, 0.7f);

            // Damage enemies in whip area.
            float attackDuration = Visuals.animator.GetAnimationLength("AttackWithWhip");
            float timer = 0f;
            HashSet<Enemy> hitEnemies = new HashSet<Enemy>();

            while (timer < attackDuration) {
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
                    if (enemy == null || hitEnemies.Contains(enemy)) {
                        continue;
                    }

                    enemy.Health.TakeDamage(whipDamage);
                    hitEnemies.Add(enemy);
                }

                timer += Time.deltaTime;
                yield return null;
            }
            
            _isAttacking = false;
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
            velocity.y = _maxJumpVelocity;
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
            velocity = knockback;
            _knockbackTimer = knockbackDuration;

            if (!ReferenceEquals(stateMachine.CurrentState, inAirState)) {
                stateMachine.AttemptToChangeState(inAirState);
            }
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
            if (_fallThroughCoroutine != null) {
                StopCoroutine(_fallThroughCoroutine);
            }

            _fallThroughCoroutine = StartCoroutine(FallThroughPlatformWindow(duration));
        }

        private IEnumerator FallThroughPlatformWindow(float duration) {
            Physics.collisionInfo.fallingThroughPlatform = true;
            yield return new WaitForSeconds(duration);
            Physics.collisionInfo.fallingThroughPlatform = false;
            _fallThroughCoroutine = null;
        }

        // TODO: Make it so that we can show debug info for whatever entity we select.
        private void OnGUI() {
            string[] debugInfo = {
                "--- Player info ---",
                "State: " + stateMachine.CurrentState.GetType().Name,
                "--- Physics info --- ",
                "Requested Velocity X: " + velocity.x,
                "Requested Velocity Y: " + velocity.y,
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
                GUI.Label(new Rect(8, 52 + 16 * i, 300, 22), debugInfo[i]);
            }
        }

    }

}
