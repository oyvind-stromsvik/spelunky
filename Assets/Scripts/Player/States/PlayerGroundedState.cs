using UnityEngine;

namespace Spelunky {

    /// <summary>
    /// The state for whenever we're on the ground.
    /// </summary>
    public class PlayerGroundedState : PlayerState {

        [Header("Animations")]
        public SpriteAnimation crawlAnimation;
        public SpriteAnimation pushAnimation;
        public SpriteAnimation runAnimation;
        public SpriteAnimation duckAnimation;
        public SpriteAnimation idleAnimation;
        public SpriteAnimation lookUpAnimation;
        public SpriteAnimation unsteadyAnimation;

        public IPushable pushingBlock;

        public override void EnterState() {
            player.Physics.OnCollisionExitEvent.AddListener(OnEntityPhysicsCollisionExit);

            if (ReferenceEquals(player.stateMachine.PreviousState, player.inAirState)) {
                player.Audio.Play(player.Audio.landClip);
            }
        }

        public override void ExitState() {
            player.Physics.OnCollisionExitEvent.RemoveListener(OnEntityPhysicsCollisionExit);
            player.Physics.canPushBlocks = false;

            if (pushingBlock != null) {
                pushingBlock = null;
            }
        }

        public override void UpdateState() {
            player.groundedGraceTimer = 0;
            player.Physics.canPushBlocks = player.directionalInput.x != 0 && player.directionalInput.y >= 0;

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
            // When pushing a block, use a fixed push speed for even pixel stepping.
            if (pushingBlock != null) {
                if (player.directionalInput.x == 0) {
                    velocity.x = 0;
                } else {
                    // Use a fixed push speed to avoid uneven pixel stepping while pushing.
                    velocity.x = player.pushBlockSpeed * Mathf.Sign(player.directionalInput.x);
                }
            }

            velocity.y = 0;
        }

        private void HandleHorizontalInput() {
            if (player.directionalInput.x != 0) {
                if (player.directionalInput.y < 0) {
                    player.Visuals.animator.Play(crawlAnimation, 1, false);
                }
                else if (player.Physics.collisionInfo.colliderHorizontal != null) {
                    IPushable pushable = player.Physics.collisionInfo.colliderHorizontal.GetComponent<IPushable>();
                    if (pushable != null) {
                        pushingBlock = pushable;
                        player.Visuals.animator.Play(pushAnimation, 1, false);
                    }
                    else {
                        player.Visuals.animator.Play(runAnimation, 1, false);
                    }
                }
                else {
                    player.Visuals.animator.Play(runAnimation, 1, false);
                }

                if (player.sprinting) {
                    player.Visuals.animator.fps = 18;
                }
            }
            else {
                if (player.directionalInput.y < 0) {
                    player.Visuals.animator.Play(duckAnimation);
                }
                else {
                    player.Visuals.animator.Play(idleAnimation, 1, false);
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
                    player.Visuals.animator.Play(lookUpAnimation);
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
            RaycastHit2D hitCenter = Physics2D.Raycast(player.transform.position + Vector3.up, Vector2.down, 2, player.Physics.blockingMask);
            Debug.DrawRay(player.transform.position + Vector3.up, Vector2.down * 2, Color.magenta);

            Vector3 offsetForward = new Vector3(player.Physics.Collider.size.x * player.Visuals.facingDirection / 2f, 1, 0);
            RaycastHit2D hitForward = Physics2D.Raycast(player.transform.position + offsetForward, Vector2.down, 2, player.Physics.blockingMask);
            Debug.DrawRay(player.transform.position + offsetForward, Vector2.down * 2, Color.green);

            // Play unsteady animation
            if (player.Physics.collisionInfo.down && hitCenter.collider == null && hitForward.collider == null) {
                if (player.directionalInput.y >= 0) {
                    player.Visuals.animator.Play(unsteadyAnimation, 1, false);
                }
            }
        }

        private void OnEntityPhysicsCollisionExit(CollisionInfo collisionInfo) {
            if (pushingBlock == null) {
                return;
            }

            if (collisionInfo.left || collisionInfo.right) {
                pushingBlock = null;
            }
        }

    }

}
