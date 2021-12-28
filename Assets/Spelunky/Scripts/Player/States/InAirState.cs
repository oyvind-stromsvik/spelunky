using UnityEngine;

namespace Spelunky {

    public class InAirState : State {
        [HideInInspector] public RaycastHit2D lastEdgeGrabRayCastHit;

        public override void Awake() {
            base.Awake();
            player.Physics.OnCollisionEnterEvent.AddListener(OnEntityPhysicsCollisionEnter);
        }

        public override void OnDirectionalInput(Vector2 input) {
            base.OnDirectionalInput(input);

            if (player.directionalInput.y > 0) {
                player.stateMachine.AttemptToChangeState(player.climbingState);
            }
        }

        public override void OnJumpInputDown() {
            if (player.groundedGraceTimer > player.groundedGracePeriod) {
                return;
            }

            base.OnJumpInputDown();
        }

        private void Update() {
            if (player.Physics.collisionInfo.becameGroundedThisFrame) {
                player.stateMachine.AttemptToChangeState(player.groundedState);
            }

            player.groundedGraceTimer += Time.deltaTime;

            HandleEdgeGrabbing();

            player.Visuals.animator.Play("Jump");

            TryToClimb();
        }

        private void HandleEdgeGrabbing() {
            Vector2 direction = Vector2.right * player.Visuals.facingDirection;

            // This was just what felt right.
            // TODO: Maybe this isn't the best suited for when we're grabbing with the glove. Investigate this.
            const float yOffset = 10f;
            const float rayLength = 9f;

            RaycastHit2D hit = Physics2D.Raycast(transform.position + Vector3.up * yOffset, direction, rayLength, player.edgeGrabLayerMask);
            Debug.DrawRay(transform.position + Vector3.up * yOffset, direction * rayLength, Color.cyan);

            // We're currently trying to move into a wall either on the left or on the right.
            bool movingIntoWallOnTheLeft = player.Physics.collisionInfo.left && player.directionalInput.x < 0 && !player.Visuals.isFacingRight;
            bool movingIntoWallOnTheRight = player.Physics.collisionInfo.right && player.directionalInput.x > 0 && player.Visuals.isFacingRight;

            if ((movingIntoWallOnTheLeft || movingIntoWallOnTheRight) && player.velocity.y < 0 && hit.collider != null) {
                // If we have the glove we can grab anything.
                if (player.Inventory.hasClimbingGlove) {
                    // TODO: How do we pass data to a state?
                    player.hangingState.colliderToHangFrom = hit.collider;
                    player.hangingState.grabbedWallUsingGlove = true;
                    player.stateMachine.AttemptToChangeState(player.hangingState);
                }
                // Otherwise we can only grab ledges (tile corners).
                // lastEdgeGrabRayCastHit.collider == null ensures we'll only grab
                // an actual ledge with air above it and only when we're falling downwards.
                else if (lastEdgeGrabRayCastHit.collider == null) {
                    player.hangingState.colliderToHangFrom = hit.collider;
                    player.stateMachine.AttemptToChangeState(player.hangingState);
                }
            }

            lastEdgeGrabRayCastHit = hit;
        }

        private void TryToClimb() {
            player.stateMachine.AttemptToChangeState(player.climbingState);
        }

        private bool hitHead;
        private bool bouncedOnEnemy;

        private void OnEntityPhysicsCollisionEnter(CollisionInfo collisionInfo) {
            if (collisionInfo.down && collisionInfo.colliderVertical.CompareTag("Enemy")) {
                collisionInfo.colliderVertical.GetComponent<EntityHealth>().TakeDamage(1);
                bouncedOnEnemy = true;
            }

            if (collisionInfo.up) {
                hitHead = true;
            }
        }

        public override void ChangePlayerVelocity(ref Vector2 velocity) {
            if (hitHead) {
                player.velocity.y = 0;
                hitHead = false;
            }

            if (bouncedOnEnemy) {
                velocity.y = player._maxJumpVelocity;
                bouncedOnEnemy = false;
            }
        }

    }

}
