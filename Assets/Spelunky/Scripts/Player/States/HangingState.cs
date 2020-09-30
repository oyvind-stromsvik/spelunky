using UnityEngine;

namespace Spelunky {
    public class HangingState : State {

        public bool grabbedWallUsingGlove;
        public Collider2D colliderToHangFrom;

        public override bool CanEnter() {
            // The tile we were going to hang from was destroyed.
            if (colliderToHangFrom == null) {
                return false;
            }

            return true;
        }

        public override void Enter() {
            Vector2 hangPosition = new Vector2(transform.position.x, colliderToHangFrom.transform.position.y + 6);
            if (player.Visuals.isFacingRight) {
                if (colliderToHangFrom.transform.position.x < player.transform.position.x) {
                    player.Visuals.FlipCharacter();
                }
            }
            else {
                if (colliderToHangFrom.transform.position.x > player.transform.position.x) {
                    player.Visuals.FlipCharacter();
                }
            }

            if (grabbedWallUsingGlove) {
                hangPosition.y = transform.position.y;
            }

            transform.position = new Vector2(hangPosition.x, hangPosition.y);

            player.Visuals.animator.Play("Hang", true);

            player.Audio.Play(player.Audio.grabClip);
        }

        private void Update() {
            // The tile we're hanging from was destroyed.
            if (colliderToHangFrom == null) {
                player.stateMachine.AttemptToChangeState(player.inAirState);
                return;
            }

            if (player.directionalInput.y != 0) {
                player._lookTimer += Time.deltaTime;
                if (player.directionalInput.y > 0) {
                    player.Visuals.animator.Play("HangLookUp");
                }
                if (player._lookTimer > player._timeBeforeLook) {
                    float offset = Mathf.Lerp(0, 64f * Mathf.Sign(player.directionalInput.y), Time.deltaTime * 128);
                    player.cam.SetVerticalOffset(offset);
                }
            }
            else {
                player._lookTimer = 0;
                player.cam.SetVerticalOffset(0);
                player.Visuals.animator.Play("Hang");
            }
        }

        public override void OnDirectionalInput(Vector2 input) {
            input.x = 0;
            base.OnDirectionalInput(input);
        }

        public override void ChangePlayerVelocity(ref Vector2 velocity) {
            velocity = Vector2.zero;
        }
    }
}
