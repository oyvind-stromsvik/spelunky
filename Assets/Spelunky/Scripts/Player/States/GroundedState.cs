using UnityEngine;

namespace Spelunky {

    public class GroundedState : State {

        public override void Awake() {
            base.Awake();
            player.Physics.OnCollisionEvent.AddListener(OnCollision);
        }

        public override void Enter() {
            if (player.stateMachine.PreviousState == player.inAirState) {
                player.Audio.Play(player.Audio.landClip);
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
        }

        private bool pushing;

        private void HandleHorizontalInput() {
            if (player.directionalInput.x != 0) {
                if (pushing) {
                    player.Visuals.animator.Play("Push", 1, false);
                }
                else if (player.directionalInput.y < 0) {
                    player.Visuals.animator.Play("Crawl", 1, false);
                }
                else {
                    player.Visuals.animator.Play("Run", 1, false);
                }

                if (player.sprinting) {
                    player.Visuals.animator.fps = 18;
                }

                pushing = false;
            }
            else {
                if (player.directionalInput.y < 0) {
                    player.Visuals.animator.Play("Duck");
                }
                else {
                    player.Visuals.animator.Play("Idle", 1, false);
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
                    player.Visuals.animator.Play("Unsteady", 1, false);
                }
            }
        }

        public void OnCollision(CollisionInfo collisionInfo) {
            // TODO: The block gets stuck to the player. The same happens to the Cavemen. Fix it!
            // TODO: How do we make the block stop immediately when we stop pushing?
            if (collisionInfo.collider.CompareTag("Block")) {
                print("block");
                collisionInfo.collider.GetComponent<Block>().Push(player.pushBlockSpeed * player.directionalInput.x);
            }

            if (collisionInfo.left || collisionInfo.right) {
                pushing = true;
            }
        }

        public override void ChangePlayerVelocityAfterMove(ref Vector2 velocity) {
            velocity.y = 0;
        }
    }

}
