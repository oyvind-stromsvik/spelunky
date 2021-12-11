using UnityEngine;

namespace Spelunky {

    [RequireComponent(typeof(Player))]
    public abstract class State : MonoBehaviour {
        [HideInInspector] public Player player;

        public virtual void Awake() {
            player = GetComponent<Player>();
            enabled = false;
        }

        public virtual bool CanEnter() {
            return true;
        }

        public virtual void Enter() {
        }

        public virtual void Exit() {
        }

        public virtual void OnDirectionalInput(Vector2 input) {
            player.directionalInput = input;

            if (player.directionalInput.x > 0 && !player.Visuals.isFacingRight) {
                player.Visuals.FlipCharacter();
            }
            else if (player.directionalInput.x < 0 && player.Visuals.isFacingRight) {
                player.Visuals.FlipCharacter();
            }
        }

        public virtual void OnJumpInputDown() {
            player.velocity.y = player._maxJumpVelocity;
            if ((player.stateMachine.CurrentState == player.climbingState || player.stateMachine.CurrentState == player.hangingState) && player.directionalInput.y < 0) {
                player.velocity.y = 0;
            }

            if (player.directionalInput.y < 0 && player.Physics.collisionInfo.down && player.Physics.collisionInfo.collider.CompareTag("OneWayPlatform")) {
                player.velocity.y = 0;
                player.Physics.collisionInfo.fallingThroughPlatform = true;
            }

            Invoke("ResetFallingThroughPlatform", .1f);

            player.Audio.Play(player.Audio.jumpClip);
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

        public virtual void OnAttackInputDown() {
            player.Attack();
        }

        public void ResetFallingThroughPlatform() {
            player.Physics.collisionInfo.fallingThroughPlatform = false;
        }

        public virtual void ChangePlayerVelocity(ref Vector2 velocity) {
        }

        public virtual bool LockInput() {
            return false;
        }
    }

}
