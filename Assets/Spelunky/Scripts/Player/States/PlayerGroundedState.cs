using UnityEngine;

namespace Spelunky {

    /// <summary>
    /// The state for whenever we're on the ground.
    /// </summary>
    public class PlayerGroundedState : PlayerState {

        public Block pushingBlock;

        public override void EnterState() {
            player.Physics.OnCollisionEnterEvent.AddListener(OnEntityPhysicsCollisionEnter);
            player.Physics.OnCollisionExitEvent.AddListener(OnEntityPhysicsCollisionExit);

            if (ReferenceEquals(player.stateMachine.PreviousState, player.inAirState)) {
                player.Audio.Play(player.Audio.landClip);
            }
        }

        public override void ExitState() {
            player.Physics.OnCollisionEnterEvent.RemoveListener(OnEntityPhysicsCollisionEnter);
            player.Physics.OnCollisionExitEvent.RemoveListener(OnEntityPhysicsCollisionExit);

            if (pushingBlock) {
                pushingBlock.Push(0);
                pushingBlock = null;
            }
        }

        public override void UpdateState() {
            player.groundedGraceTimer = 0;

            HandleHorizontalInput();
            HandleLookUpDown();
            HandleUnsteady();

            if (!player.Physics.collisionInfo.down) {
                if (player.directionalInput.y < 0) {
                    if (player.stateMachine.AttemptToChangeState(player.crawlToHangState)) {
                        return;
                    }
                }

                player.stateMachine.AttemptToChangeState(player.inAirState);
                return;
            }

            if (player.directionalInput.y > 0) {
                player.stateMachine.AttemptToChangeState(player.climbingState);
            }
            else if (player.directionalInput.y < 0 && player.Physics.collisionInfo.down && player.Physics.collisionInfo.colliderVertical.CompareTag("OneWayPlatform")) {
                player.stateMachine.AttemptToChangeState(player.climbingState);
            }
        }

        public override void ChangePlayerVelocity(ref Vector2 velocity) {
            if (pushingBlock != null && player.directionalInput.x == 0) {
                velocity.x = 0;
            }

            velocity.y = 0;
        }

        private void HandleHorizontalInput() {
            if (player.directionalInput.x != 0) {
                if (player.directionalInput.y < 0) {
                    player.Visuals.animator.Play("Crawl", 1, false);
                }
                else if (player.Physics.collisionInfo.colliderHorizontal != null && player.Physics.collisionInfo.colliderHorizontal.CompareTag("Block")) {
                    pushingBlock = player.Physics.collisionInfo.colliderHorizontal.GetComponent<Block>();
                    pushingBlock.Push(player.pushBlockSpeed * player.directionalInput.x);
                    player.Visuals.animator.Play("Push", 1, false);
                }
                else {
                    player.Visuals.animator.Play("Run", 1, false);
                }

                if (player.sprinting) {
                    player.Visuals.animator.fps = 18;
                }
            }
            else {
                if (pushingBlock != null) {
                    pushingBlock.Push(0);
                }

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

        private void OnEntityPhysicsCollisionEnter(CollisionInfo collisionInfo) {
            // TODO: This is a hack to avoid us getting dragged after enemies due to some weird interaction with the
            // collision detection. See the TODO in Player.CalculateVelocity() for more information.
            if ((collisionInfo.left || collisionInfo.right) && collisionInfo.colliderHorizontal.CompareTag("Enemy")) {
                player.velocity.x = 0;
            }
        }

        private void OnEntityPhysicsCollisionExit(CollisionInfo collisionInfo) {
            if (pushingBlock == null) {
                return;
            }

            if ((collisionInfo.left || collisionInfo.right) && collisionInfo.colliderHorizontal.CompareTag("Block")) {
                // Just to see if things work as expected.
                if (pushingBlock != collisionInfo.colliderHorizontal.GetComponent<Block>()) {
                    Debug.LogError("Trying to exit the collision from a different block than the one we entered the collision with!");
                }

                pushingBlock.Push(0);
                pushingBlock = null;
            }
        }

    }

}