using UnityEngine;

namespace Spelunky {
    public class InAirState : State {

        [HideInInspector] public RaycastHit2D lastEdgeGrabRayCastHit;

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
            if (player.PhysicsObject.collisions.becameGroundedThisFrame) {
                player.stateMachine.AttemptToChangeState(player.groundedState);
            }

            player.groundedGraceTimer += Time.deltaTime;

            HandleEdgeGrabbing();

            player.graphics.animator.Play("Jump");

            TryToClimb();
        }

        private void HandleEdgeGrabbing() {
            Vector2 direction = Vector2.right * player.facingDirection;

            RaycastHit2D hit = Physics2D.Raycast(transform.position + Vector3.up * 13, direction, 9, player.edgeGrabLayerMask);
            Debug.DrawRay(transform.position + Vector3.up * 13, direction * 9, Color.cyan);
            // Grab edge.
            if ((player.PhysicsObject.collisions.left || player.PhysicsObject.collisions.right) && player.velocity.y < 0 && hit.collider != null && lastEdgeGrabRayCastHit.collider == null) {
                if ((player.directionalInput.x > 0 && player.isFacingRight) || (player.directionalInput.x < 0 && !player.isFacingRight)) {
                    player.audio.Play(player.audio.grabClip);
                    player.hangingState.colliderToHangFrom = hit.collider;
                    player.stateMachine.AttemptToChangeState(player.hangingState);
                }
            }

            lastEdgeGrabRayCastHit = hit;
        }

        private void TryToClimb() {
            player.stateMachine.AttemptToChangeState(player.climbingState);
        }

    }
}
