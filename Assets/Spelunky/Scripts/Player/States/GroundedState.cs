using UnityEngine;

namespace Spelunky {
    public class GroundedState : State {

        public override void Enter() {
            base.Enter();

            if (player.stateMachine.LastState == player.inAirState) {
                player.audio.Play(player.audio.landClip);
            }
        }

        public void Update() {
            player.groundedGraceTimer = 0;

            HandleHorizontalInput();
            HandleLookUpDown();
            HandleUnsteady();

            if (!player.physicsObject.collisions.below) {
                if (player.directionalInput.y < 0) {
                    player.stateMachine.AttemptToChangeState(player.crawlToHangState);
                    return;
                }

                player.stateMachine.AttemptToChangeState(player.inAirState);
                return;
            }

            if (player.directionalInput.y > 0) {
                player.stateMachine.AttemptToChangeState(player.climbingState);
            }
            else if (player.directionalInput.y < 0 && player.physicsObject.collisions.below && player.physicsObject.collisions.colliderBelow.CompareTag("OneWayPlatform")) {
                player.stateMachine.AttemptToChangeState(player.climbingState);
            }
        }

        private void HandleHorizontalInput() {
            if (player.directionalInput.x != 0) {
                if (player.directionalInput.y < 0) {
                    player.graphics.animator.Play("Crawl");
                }
                else {
                    player.graphics.animator.Play("Run");
                }

                if (player.directionalInput.y < 0) {
                    player.graphics.animator.fps = 12;
                }
                else if (player.sprinting) {
                    player.graphics.animator.fps = 18;
                }
                else {
                    player.graphics.animator.fps = 12;
                }
            }
            else {
                if (player.directionalInput.y < 0) {
                    player.graphics.animator.Play("Duck");
                }
                else {
                    player.graphics.animator.Play("Idle");
                }
            }
        }

        private void HandleLookUpDown() {
            if (player.directionalInput.x != 0) {
                return;
            }

            if (player.directionalInput.y != 0) {
                player._lookTimer += Time.deltaTime;
                if (player.directionalInput.y > 0) {
                    player.graphics.animator.Play("LookUp");
                }
                if (player._lookTimer > player._timeBeforeLook) {
                    float offset = Mathf.Lerp(0, 64f * Mathf.Sign(player.directionalInput.y), Time.deltaTime * 128);
                    player.cam.SetVerticalOffset(offset);
                }
            }
            else {
                player._lookTimer = 0;
                player.cam.SetVerticalOffset(0);
            }
        }

        private void HandleUnsteady() {
            RaycastHit2D hitCenter = Physics2D.Raycast(player.transform.position + Vector3.up, Vector2.down, 2, player.physicsObject.collisionMask);
            Debug.DrawRay(player.transform.position + Vector3.up, Vector2.down * 2, Color.magenta);

            Vector3 offsetForward = new Vector3(6 * player.facingDirection, 1, 0);
            RaycastHit2D hitForward = Physics2D.Raycast(player.transform.position + offsetForward, Vector2.down, 2, player.physicsObject.collisionMask);
            Debug.DrawRay(player.transform.position + offsetForward, Vector2.down * 2, Color.green);

            // Play unsteady animation
            if (player.physicsObject.collisions.below && hitCenter.collider == null && hitForward.collider == null) {
                if (player.directionalInput.y >= 0) {
                    player.graphics.animator.Play("Unsteady");
                }
            }
        }
    }
}
