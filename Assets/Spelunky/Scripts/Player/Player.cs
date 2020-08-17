using System;
using UnityEngine;

namespace Spelunky {

	public class Player : MonoBehaviour, IObjectController {

		[Header("Components")]
		public PhysicsObject physicsObject;
		public PlayerInput input;
		public EntityVisuals graphics;
		public new PlayerAudio audio;
		public PlayerInventory inventory;
		public EntityHealth health;

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
		public float pushBlockSpeed;

		[HideInInspector] public bool recentlyJumped;
		[HideInInspector] public float _lastJumpTimer;

		[HideInInspector] public float _lookTimer;
		[HideInInspector] public float _timeBeforeLook = 1f;

		public StateMachine stateMachine = new StateMachine();

		private float _stunDuration;

		private void Awake() {
			_gravity = -(2 * maxJumpHeight) / Mathf.Pow (timeToJumpApex, 2);
			_maxJumpVelocity = Mathf.Abs(_gravity) * timeToJumpApex;
			_minJumpVelocity = Mathf.Sqrt (2 * Mathf.Abs (_gravity) * minJumpHeight);

			health.HealthChanged.AddListener(OnHealthChanged);
		}

		private void Start() {
			stateMachine.AttemptToChangeState(groundedState);
		}

		private void Update() {
			if (directionalInput.x > 0 && !graphics.isFacingRight) {
				graphics.FlipCharacter();
			}
			else if (directionalInput.x < 0 && graphics.isFacingRight) {
				graphics.FlipCharacter();
			}

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

			if (physicsObject.collisionInfo.up || physicsObject.collisionInfo.down) {
				velocity.y = 0;
			}

			_stunDuration -= Time.deltaTime;
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
			if (_stunDuration <= 0f) {
				float targetVelocityX = directionalInput.x * _speed;
				velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref _velocityXSmoothing, accelerationTime);
			}

			velocity.y += _gravity * Time.deltaTime;

			stateMachine.CurrentState.ChangePlayerVelocity(ref velocity);
		}

		public void ThrowBomb() {
			if (inventory.numberOfBombs <= 0) {
				return;
			}

			inventory.UseBomb();

			Bomb bombInstance = Instantiate(bomb, transform.position + Vector3.up * 2f, Quaternion.identity);
			Vector2 bombVelocity = new Vector2(256 * graphics.facingDirection, 128);
			if (directionalInput.y == 1) {
				bombVelocity = new Vector2(128 * graphics.facingDirection, 256);
			}
			else if (directionalInput.y == -1) {
				if (physicsObject.collisionInfo.down) {
					bombVelocity = new Vector2(64 * graphics.facingDirection, 0);
				}
				else {
					bombVelocity = new Vector2(128 * graphics.facingDirection, -256);
				}
			}

			bombInstance.SetVelocity(bombVelocity);
		}

		public void ThrowRope() {
			if (inventory.numberOfRopes <= 0) {
				return;
			}

			inventory.UseRope();

			Rope ropeInstance = Instantiate(rope, transform.position, Quaternion.identity);
			if (stateMachine.CurrentState == groundedState && directionalInput.y < 0) {
				ropeInstance.placePosition = transform.position + (graphics.facingDirection * Vector3.right * Tile.Width);
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

		public void Splat() {
			stateMachine.AttemptToChangeState(splatState);
		}

		public bool IgnoreCollider(Collider2D collider, CollisionDirection direction) {
			// One way platform handling.
			if (collider.CompareTag("OneWayPlatform")) {
				// Always ignore them if we're colliding horizontally.
				if (direction == CollisionDirection.Left || direction == CollisionDirection.Right) {
					return true;
				}

				// If we're colliding vertically ignore them if we're going up or if we're passing through them.
				if (direction == CollisionDirection.Up || direction == CollisionDirection.Down) {
					if (direction == CollisionDirection.Up) {
						return true;
					}
					if (physicsObject.collisionInfo.fallingThroughPlatform) {
						return true;
					}
				}
			}

			if (collider.CompareTag("Enemy") && direction == CollisionDirection.Down) {
				collider.GetComponent<Enemy>().EntityHealth.TakeDamage(1);
			}

			return false;
		}

		public void OnCollision(CollisionInfo collisionInfo) {
		}

		public void TakeDamage(int damage, CollisionDirection direction) {
			if (GetComponent<EntityHealth>().IsInvulernable) {
				return;
			}

			GetComponent<EntityHealth>().TakeDamage(damage);
			velocity = new Vector2(128 * graphics.facingDirection * -1, 64);
			_stunDuration = 0.5f;
		}

		private void OnHealthChanged() {
			if (health.CurrentHealth <= 0) {
				stateMachine.AttemptToChangeState(splatState);
			}
		}

		public void UpdateVelocity(ref Vector2 velocity) {

		}
	}
}
