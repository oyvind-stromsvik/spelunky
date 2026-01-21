using UnityEngine;

namespace Spelunky {

    /// <summary>
    /// The state we're in when we hang from a tile or block.
    /// </summary>
    public class PlayerHangingState : PlayerState {

        public bool grabbedWallUsingGlove;
        public Collider2D colliderToHangFrom;

        public override bool CanEnterState() {
            // We can't enter this state without something to hang from.
            if (colliderToHangFrom == null) {
                return false;
            }

            return true;
        }

        public override void EnterState() {
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

            // Set our sprite to the hang sprite immediately when we enter this state because the conditionals we
            // currently have in UpdateState() could lead to us not actually changing to the Hang sprite if we have tiny
            // amount of vertical directional input.
            // TODO: This can probably be fixed properly in the UpdateState() instead?
            player.Visuals.animator.Play("Hang");

            player.Audio.Play(player.Audio.grabClip);
        }

        public override void UpdateState() {
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
            // We have to disallow horizontal input even though we zero the velocity in this state, otherwise we whould
            // be able to change our facing direction which would look completely wrong. We still allow vertical input
            // because we use that for looking up or down.
            input.x = 0;
            base.OnDirectionalInput(input);
        }

        public override void ChangePlayerVelocity(ref Vector2 velocity) {
            velocity = Vector2.zero;
        }

        public override void OnAttackInputDown() {
            base.OnAttackInputDown();
            // If we attack while hanging we should fall.
            player.stateMachine.AttemptToChangeState(player.inAirState);
        }

    }

}