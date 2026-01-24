using UnityEngine;

namespace Spelunky {

    /// <summary>
    /// The state for whenever we're in the air, whether it's from jumping, falling or anything else.
    /// </summary>
    public class PlayerInAirState : PlayerState {

        private RaycastHit2D _lastEdgeGrabRayCastHit;
        private bool _hitHead;

        public override void EnterState() {
            player.Physics.OnCollisionEnterEvent.AddListener(OnEntityPhysicsCollisionEnter);
        }

        public override void ExitState() {
            player.Physics.OnCollisionEnterEvent.RemoveListener(OnEntityPhysicsCollisionEnter);
        }

        public override void UpdateState() {
            player.groundedGraceTimer += Time.deltaTime;

            // TODO: We currently only have a single sprite for anything "air" related. Later on we would probably
            // created animations for jumping, falling etc. It's especially important to have something for when we're
            // ragdolled.
            player.Visuals.animator.Play("Jump");

            HandleEdgeGrabbing();
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

        public override void ChangePlayerVelocity(ref Vector2 velocity) {
            if (_hitHead) {
                velocity.y = 0;
                _hitHead = false;
            }
        }

        private void HandleEdgeGrabbing() {
            Vector2 direction = Vector2.right * player.Visuals.facingDirection;

            // This was just what felt right.
            // TODO: Maybe this isn't the best suited for when we're grabbing with the glove. Investigate this.
            const float yOffset = 12f;
            // This should only stick 1 "pixel" out from our collider so that we can grab the tiniest of ledges.
            // TODO: This doesn't currently work. We're able to stand on any width of ledge, but we're not able to grab
            // very tiny ledges. We need a better check here. There's also a bug with CrawlToHang if we're on really
            // tiny ledges where it will hang on the ledge above us instead of the one below us.
            const float rayLength = 5f;

            RaycastHit2D hit = Physics2D.Raycast(transform.position + Vector3.up * yOffset, direction, rayLength, player.edgeGrabLayerMask);
            Debug.DrawRay(transform.position + Vector3.up * yOffset, direction * rayLength, Color.cyan);

            // We're currently trying to move into a wall either on the left or on the right.
            bool movingIntoWallOnTheLeft = player.Physics.collisionInfo.left && player.directionalInput.x < 0 && !player.Visuals.isFacingRight;
            bool movingIntoWallOnTheRight = player.Physics.collisionInfo.right && player.directionalInput.x > 0 && player.Visuals.isFacingRight;

            if ((movingIntoWallOnTheLeft || movingIntoWallOnTheRight) && player.velocity.y < 0 && CanHangFrom(hit.collider)) {
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
                else if (_lastEdgeGrabRayCastHit.collider == null) {
                    player.hangingState.colliderToHangFrom = hit.collider;
                    player.stateMachine.AttemptToChangeState(player.hangingState);
                }
            }

            _lastEdgeGrabRayCastHit = hit;
        }

        private bool CanHangFrom(Collider2D collider) {
            if (collider == null) {
                return false;
            }

            if (collider.CompareTag("OneWayPlatform")) {
                return false;
            }

            return true;
        }

        private void OnEntityPhysicsCollisionEnter(CollisionInfo collisionInfo) {
            if (collisionInfo.becameGroundedThisFrame) {
                if (collisionInfo.colliderVertical.CompareTag("Spikes")) {
                    player.Splat();
                }
                else {
                    player.stateMachine.AttemptToChangeState(player.groundedState);
                }
            }

            if (collisionInfo.up) {
                // Same reasoning as above for handling velocity changes after Move().
                _hitHead = true;
            }
        }

    }

}
