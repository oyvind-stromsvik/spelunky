using UnityEngine;

namespace Spelunky {
    public abstract class State : MonoBehaviour {

        [HideInInspector] public Player player;

        public virtual bool CanEnter() {
            return true;
        }

        public virtual void Enter() {
            enabled = true;
        }

        public virtual void Exit() {
            enabled = false;
        }

        public virtual void OnDirectionalInput(Vector2 input) {
            player.directionalInput = input;
        }

        public virtual void OnJumpInputDown() {
            player.velocity.y = player._maxJumpVelocity;
            if ((player.stateMachine.CurrentState == player.climbingState || player.stateMachine.CurrentState == player.hangingState) && player.directionalInput.y < 0) {
                player.velocity.y = 0;
            }

            if (player.directionalInput.y < 0 && player.physicsObject.collisions.colliderBelow != null && player.physicsObject.collisions.colliderBelow.CompareTag("OneWayPlatform")) {
                player.velocity.y = 0;
                player.physicsObject.collisions.fallingThroughPlatform = true;
            }

            Invoke("ResetFallingThroughPlatform", .1f);

            player.audio.Play(player.audio.jumpClip);
            player.recentlyJumped = true;
            // Increase the grace timer so it's impossible to accidentally double jump.
            // TODO: Can the above variable be used for this purpose?
            player.groundedGraceTimer += player.groundedGracePeriod;

            player.stateMachine.AttemptToChangeState(player.inAirState);
        }

        public virtual void OnJumpInputUp() {
            if (player.velocity.y > player._minJumpVelocity) {
                player.velocity.y = player._minJumpVelocity;
            }
        }

        public virtual void OnBombInputDown() {
            player.ThrowBomb();
        }

        public virtual void OnRopeInputDown() {
            player.ThrowRope();
        }

        public virtual void OnUseInputDown() {
            player.Use();
        }

        public void ResetFallingThroughPlatform() {
            player.physicsObject.collisions.fallingThroughPlatform = false;
        }

        public virtual void ChangePlayerVelocity(ref Vector2 velocity) {
        }

        public virtual bool LockInput() {
            return false;
        }
    }
}
