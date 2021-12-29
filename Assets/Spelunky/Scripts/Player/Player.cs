using System.Collections;
using UnityEngine;

namespace Spelunky {

    public class Player : Entity {

        public PlayerInput Input { get; private set; }
        public PlayerAudio Audio { get; private set; }
        public PlayerInventory Inventory { get; private set; }

        public Collider2D whipCollider;

        [Header("States")]
        public GroundedState groundedState;
        public InAirState inAirState;
        public HangingState hangingState;
        public ClimbingState climbingState;
        public CrawlToHangState crawlToHangState;
        public EnterDoorState enterDoorState;
        public SplatState splatState;

        [Header("Crap")]
        public LayerMask edgeGrabLayerMask;
        public CameraFollow cam;
        public Exit _exitDoor;

        [Header("Abilities")]
        public Bomb bomb;
        public Rope rope;

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

        public override void Awake() {
            base.Awake();

            _gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
            _maxJumpVelocity = Mathf.Abs(_gravity) * timeToJumpApex;
            _minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(_gravity) * minJumpHeight);

            Input = GetComponent<PlayerInput>();
            Audio = GetComponent<PlayerAudio>();
            Inventory = GetComponent<PlayerInventory>();

            Health.HealthChangedEvent.AddListener(OnHealthChanged);
        }

        private void Start() {
            stateMachine.AttemptToChangeState(groundedState);
        }

        private void Update() {
            stateMachine.CurrentState.UpdateState();

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

            stateMachine.CurrentState.ChangePlayerVelocityAfterMove(ref velocity);
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
            float targetVelocityX = directionalInput.x * _speed;
            velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref _velocityXSmoothing, accelerationTime);

            velocity.y += _gravity * Time.deltaTime;

            stateMachine.CurrentState.ChangePlayerVelocity(ref velocity);
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

            bombInstance._velocity = bombVelocity;
        }

        public void ThrowRope() {
            if (Inventory.numberOfRopes <= 0) {
                return;
            }

            Inventory.UseRope();

            Rope ropeInstance = Instantiate(rope, transform.position, Quaternion.identity);
            if (stateMachine.CurrentState == groundedState && directionalInput.y < 0) {
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
            whipCollider.enabled = true;

            Visuals.animator.PlayOnceUninterrupted("AttackWithWhip");
            Audio.Play(Audio.whipClip, 0.7f);

            yield return new WaitForSeconds(Visuals.animator.GetAnimationLength("AttackWithWhip"));

            _isAttacking = false;
            whipCollider.enabled = false;
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

        private void OnHealthChanged() {
            if (Health.CurrentHealth <= 0) {
                stateMachine.AttemptToChangeState(splatState);
            }
        }
    }

}
