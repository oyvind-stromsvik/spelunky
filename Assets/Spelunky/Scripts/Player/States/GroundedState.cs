using UnityEngine;

namespace Spelunky {

    public class GroundedState : State {
        public Block blockToPush;
        private Block _lastBlockPushed;

        public override void Enter() {
            if (player.stateMachine.PreviousState == player.inAirState) {
                player.Audio.Play(player.Audio.landClip);
            }
        }

        public override void Exit() {
            if (blockToPush != null) {
                blockToPush.pushSpeed = 0f;
                blockToPush = null;
            }
        }

        public void Update() {
            player.groundedGraceTimer = 0;

            HandleHorizontalInput();
            HandleLookUpDown();
            HandleUnsteady();

            if (!player.Physics.collisionInfo.down) {
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
            else if (player.directionalInput.y < 0 && player.Physics.collisionInfo.down && player.Physics.collisionInfo.collider.CompareTag("OneWayPlatform")) {
                player.stateMachine.AttemptToChangeState(player.climbingState);
            }

            blockToPush = null;
            if (player.directionalInput.x < 0 && player.Physics.collisionInfo.left && player.Physics.collisionInfo.collider.CompareTag("Block")) {
                blockToPush = player.Physics.collisionInfo.collider.GetComponent<Block>();
            }

            if (player.directionalInput.x > 0 && player.Physics.collisionInfo.right && player.Physics.collisionInfo.collider.CompareTag("Block")) {
                blockToPush = player.Physics.collisionInfo.collider.GetComponent<Block>();
            }

            if (blockToPush != null) {
                blockToPush.pushSpeed = player.pushBlockSpeed * player.directionalInput.x;
            }

            if (blockToPush == null && _lastBlockPushed != null) {
                _lastBlockPushed.pushSpeed = 0f;
            }

            _lastBlockPushed = blockToPush;
        }

        private void HandleHorizontalInput() {
            if (player.directionalInput.x != 0) {
                if (player.Physics.collisionInfo.left || player.Physics.collisionInfo.right) {
                    player.Visuals.animator.Play("Push");
                }
                else if (player.directionalInput.y < 0) {
                    player.Visuals.animator.Play("Crawl");
                }
                else {
                    player.Visuals.animator.Play("Run");
                }

                if (player.sprinting) {
                    player.Visuals.animator.fps = 18;
                }
            }
            else {
                if (player.directionalInput.y < 0) {
                    player.Visuals.animator.Play("Duck");
                }
                else {
                    player.Visuals.animator.Play("Idle");
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
                    player.Visuals.animator.Play("LookUp");
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
            RaycastHit2D hitCenter = Physics2D.Raycast(player.transform.position + Vector3.up, Vector2.down, 2, player.Physics.collisionMask);
            Debug.DrawRay(player.transform.position + Vector3.up, Vector2.down * 2, Color.magenta);

            Vector3 offsetForward = new Vector3(player.Physics.Collider.size.x * player.Visuals.facingDirection / 2f, 1, 0);
            RaycastHit2D hitForward = Physics2D.Raycast(player.transform.position + offsetForward, Vector2.down, 2, player.Physics.collisionMask);
            Debug.DrawRay(player.transform.position + offsetForward, Vector2.down * 2, Color.green);

            // Play unsteady animation
            if (player.Physics.collisionInfo.down && hitCenter.collider == null && hitForward.collider == null) {
                if (player.directionalInput.y >= 0) {
                    player.Visuals.animator.Play("Unsteady");
                }
            }
        }
    }

}
