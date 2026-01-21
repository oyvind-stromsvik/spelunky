using UnityEngine;

namespace Spelunky {

    [RequireComponent(typeof(Player))]
    public abstract class PlayerState : MonoBehaviour, IState {

        [HideInInspector] public Player player;

        /// <summary>
        /// TODO: Remove this. We need to get the player some other way and we shouldn't really need a reference to the
        /// player in here at all so that we can use the state logic for NPCs as well.
        /// </summary>
        private void Awake() {
            player = GetComponent<Player>();
            enabled = false;
        }

        /// <summary>
        /// </summary>
        /// <returns>A boolean indiciating whether we're allowed to enter the state or not.</returns>
        public virtual bool CanEnterState() {
            return true;
        }

        /// <summary>
        /// Called when we enter the state.
        /// Use this to perform any initialization logic for the state.
        /// </summary>
        public virtual void EnterState() {
        }

        /// <summary>
        /// Called when we exit the state.
        /// Use this to perform any cleanup logic for the state.
        /// </summary>
        public virtual void ExitState() {
        }

        /// <summary>
        /// Called from Update(). The only way the state should do per frame logic to avoid race conditions etc.
        /// </summary>
        public virtual void UpdateState() {
        }

        /// <summary>
        /// </summary>
        /// <param name="input"></param>
        public virtual void OnDirectionalInput(Vector2 input) {
            player.directionalInput = input;

            if (player.directionalInput.x > 0 && !player.Visuals.isFacingRight) {
                player.Visuals.FlipCharacter();
            }
            else if (player.directionalInput.x < 0 && player.Visuals.isFacingRight) {
                player.Visuals.FlipCharacter();
            }
        }

        /// <summary>
        /// </summary>
        public virtual void OnJumpInputDown() {
            player.velocity.y = player._maxJumpVelocity;
            if ((ReferenceEquals(player.stateMachine.CurrentState, player.climbingState) || ReferenceEquals(player.stateMachine.CurrentState, player.hangingState)) && player.directionalInput.y < 0) {
                player.velocity.y = 0;
            }

            if (player.directionalInput.y < 0 && player.Physics.collisionInfo.down && player.Physics.collisionInfo.colliderVertical.CompareTag("OneWayPlatform")) {
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

        /// <summary>
        /// </summary>
        public virtual void OnJumpInputUp() {
            if (player.velocity.y > player._minJumpVelocity) {
                player.velocity.y = player._minJumpVelocity;
            }
        }

        /// <summary>
        /// </summary>
        public virtual void OnBombInputDown() {
            player.ThrowBomb();
        }

        /// <summary>
        /// </summary>
        public virtual void OnRopeInputDown() {
            player.ThrowRope();
        }

        /// <summary>
        /// </summary>
        public virtual void OnUseInputDown() {
            player.Use();
        }

        /// <summary>
        /// </summary>
        public virtual void OnAttackInputDown() {
            player.Attack();
        }

        /// <summary>
        /// </summary>
        public void ResetFallingThroughPlatform() {
            player.Physics.collisionInfo.fallingThroughPlatform = false;
        }

        /// <summary>
        /// Change the player's velocity before we move (and perform collision detection).
        /// </summary>
        /// <param name="velocity"></param>
        public virtual void ChangePlayerVelocity(ref Vector2 velocity) {
        }

        /// <summary>
        /// Change the player's velocity after a move has happened.
        /// Currently used to set the y-velocity to 0 when we're grounded so that if we fall off a ledge we don't fall
        /// at terminal velocity immediately, but rather start falling from 0 y-velocity. This needs to happen after
        /// the move has happened because we need gravity to pull us down for the collision check to register us as
        /// being grounded. At least this is how I've chosen to handle this at the moment.
        /// </summary>
        /// <param name="velocity"></param>
        public virtual void ChangePlayerVelocityAfterMove(ref Vector2 velocity) {
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public virtual bool LockInput() {
            return false;
        }

    }

}