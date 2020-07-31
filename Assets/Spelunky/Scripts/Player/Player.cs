using UnityEngine;

namespace Spelunky {

	public class Player : MonoBehaviour {

		[Header("Components")]
		public PhysicsObject physicsObject;
		public PlayerInput input;
		public PlayerGraphics graphics;
		public new PlayerAudio audio;
		public PlayerItems items;

		[Header("States")]
		public GroundedState groundedState;
		public InAirState inAirState;
		public HangingState hangingState;
		public ClimbingState climbingState;
		public CrawlToHangState crawlToHangState;
		public EnterDoorState enterDoorState;

		[Header("Crap")]
		public LayerMask edgeGrabLayerMask;
		public CameraFollow cam;

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

		[HideInInspector] public bool sprinting;

		[Tooltip("The time in seconds that we are considered grounded after leaving a platform. Allows us to easier time jumps.")]
		public float groundedGracePeriod = 0.1f;
		[HideInInspector] public float groundedGraceTimer;

		private float _gravity;
		[HideInInspector] public float _maxJumpVelocity;
		[HideInInspector] public float _minJumpVelocity;
		[HideInInspector] public Vector2 velocity;
		private float _velocityXSmoothing;
		[HideInInspector] public Vector2 directionalInput;
		private float _speed;

		[HideInInspector] public bool recentlyJumped;
		[HideInInspector] public float _lastJumpTimer;

		[HideInInspector] public bool isFacingRight = false;
		[HideInInspector] public float facingDirection = -1;

		[HideInInspector] public float _lookTimer;
		[HideInInspector] public float _timeBeforeLook = 1f;

		public StateMachine stateMachine = new StateMachine();

		private void Awake() {
			_gravity = -(2 * maxJumpHeight) / Mathf.Pow (timeToJumpApex, 2);
			_maxJumpVelocity = Mathf.Abs(_gravity) * timeToJumpApex;
			_minJumpVelocity = Mathf.Sqrt (2 * Mathf.Abs (_gravity) * minJumpHeight);

			groundedState.player = this;
			groundedState.enabled = false;
			inAirState.player = this;
			inAirState.enabled = false;
			hangingState.player = this;
			hangingState.enabled = false;
			climbingState.player = this;
			climbingState.enabled = false;
			crawlToHangState.player = this;
			crawlToHangState.enabled = false;
			enterDoorState.player = this;
			enterDoorState.enabled = false;
			stateMachine.AttemptToChangeState(groundedState);
		}

		private void Update() {
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

			physicsObject.Move(velocity * Time.deltaTime);

			if (physicsObject.collisions.above || physicsObject.collisions.below) {
				velocity.y = 0;
			}
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
			velocity.x = Mathf.SmoothDamp (velocity.x, targetVelocityX, ref _velocityXSmoothing, accelerationTime);
			velocity.y += _gravity * Time.deltaTime;
			stateMachine.CurrentState.ChangePlayerVelocity(ref velocity);
		}

		public void ThrowBomb() {
			Bomb bombInstance = Instantiate(bomb, transform.position, Quaternion.identity);
			Vector2 bombVelocity = new Vector2(256 * facingDirection, 128);
			if (directionalInput.y == 1) {
				bombVelocity = new Vector2(128 * facingDirection, 256);
			}
			else if (directionalInput.y == -1) {
				if (physicsObject.collisions.below) {
					bombVelocity = Vector2.zero;
				}
				else {
					bombVelocity = new Vector2(128 * facingDirection, -256);
				}
			}

			bombInstance.SetVelocity(bombVelocity);
		}

		public void ThrowRope() {
			Rope ropeInstance = Instantiate(rope, transform.position, Quaternion.identity);
			if (stateMachine.CurrentState == groundedState && directionalInput.y < 0) {
				ropeInstance.placePosition = transform.position + (facingDirection * Vector3.right * Tile.Width);
			}
		}

		public void Use() {
			if (_exitDoor == null) {
				return;
			}

			stateMachine.AttemptToChangeState(enterDoorState);
		}

		public Exit _exitDoor;
		public void EnteredDoorway(Exit door) {
			_exitDoor = door;
		}

		public void ExitedDoorway(Exit door) {
			_exitDoor = null;
		}

	}
}
