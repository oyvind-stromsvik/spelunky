using System.Collections.Generic;
using UnityEngine;

namespace Spelunky {

    /// <summary>
    /// The state we're in when we're climbing a ladder or a rope.
    /// </summary>
    public class ClimbingState : State {
        public ContactFilter2D ladderFilter;
        public LayerMask ladderLayerMask;

        private Collider2D _closestCollider;

        public override bool CanEnter() {
            if (player.directionalInput.y == 0) {
                return false;
            }

            if (Mathf.Abs(player.directionalInput.y) < Mathf.Abs(player.directionalInput.x)) {
                return false;
            }

            if (player.recentlyJumped) {
                return false;
            }

            // Find any nearby ladder colliders.
            _closestCollider = FindClosestOverlappedLadder();
            if (_closestCollider == null) {
                return false;
            }

            Vector2 direction = Vector2.up;
            Vector3 position = transform.position + Vector3.up * 16;
            RaycastHit2D hit = Physics2D.Raycast(position, direction, 9, ladderLayerMask);
            Debug.DrawRay(position, direction * 9, Color.magenta);
            if (hit.collider == null) {
                return false;
            }

            return true;
        }

        public override void Enter() {
            player.Physics.collisionInfo.fallingThroughPlatform = true;
            float xPos = _closestCollider.transform.position.x;
            player.Visuals.animator.Play("ClimbRope");
            if (_closestCollider.CompareTag("Ladder")) {
                xPos += Tile.Width / 2f;
                player.Visuals.animator.Play("ClimbLadder");
            }

            transform.position = new Vector3(xPos, transform.position.y, 0);
            player.Audio.Play(player.Audio.grabClip);
        }

        private void Update() {
            if (player.directionalInput.y < 0 && player.Physics.collisionInfo.down && !player.Physics.collisionInfo.collider.CompareTag("OneWayPlatform")) {
                player.stateMachine.AttemptToChangeState(player.groundedState);
            }

            // Continously look for a ladder collider so that we can react accordingly.
            _closestCollider = FindClosestOverlappedLadder();
            if (_closestCollider == null) {
                // NOTE: Do we want this? It's like this in Spelunky, but isn't it better
                // for the player to have full control over when he drops from a ladder/rope?
                // He can just press jump and hold down at the same time and he will do the
                // same thing.
                player.stateMachine.AttemptToChangeState(player.inAirState);
            }
            else {
                player.Visuals.animator.Play("ClimbRope");
                if (_closestCollider.CompareTag("Ladder")) {
                    player.Visuals.animator.Play("ClimbLadder");
                }
            }

            if (player.directionalInput.y != 0) // Set the framerate of the climbing animation dynamically based on our climbing speed.
            {
                player.Visuals.animator.fps = Mathf.RoundToInt(Mathf.Abs(player.directionalInput.y).Remap(0.1f, 1.0f, 4, 18));
            }
            else {
                player.Visuals.animator.fps = 0;
            }
        }

        public override void ChangePlayerVelocity(ref Vector2 velocity) {
            velocity.y = player.directionalInput.y * player.climbSpeed;
            velocity.x = 0;

            // Raycast ahead of us and set our velocity to 0 if we are no longer on
            // a ladder.
            Vector2 direction = Vector2.down;
            Vector3 position = transform.position + Vector3.up * 16;
            if (player.directionalInput.y > 0) {
                direction = Vector2.up;
            }

            RaycastHit2D hit = Physics2D.Raycast(position, direction, 9, ladderLayerMask);
            Debug.DrawRay(position, direction * 9, Color.magenta);
            if (hit.collider == null) {
                velocity.y = 0;
            }

        }

        /// <summary>
        /// Find the closest ladder, in the horizontal direction, that we're currently overlapping.
        ///
        /// If we're standing between two ladders we want to grab the closet one, not the first one
        /// in the list as that one could be the furthest one away.
        /// </summary>
        private Collider2D FindClosestOverlappedLadder() {
            List<Collider2D> ladderColliders = new List<Collider2D>();
            player.Physics.Collider.OverlapCollider(ladderFilter, ladderColliders);
            if (ladderColliders.Count <= 0) {
                return null;
            }

            float closestDistance = Mathf.Infinity;
            Collider2D closestCollider = null;
            foreach (Collider2D ladderCollider in ladderColliders) {
                float xPos = ladderCollider.transform.position.x;
                if (ladderCollider.CompareTag("Ladder")) {
                    xPos += Tile.Width / 2f;
                }

                float currentDistance = Mathf.Abs(transform.position.x - xPos);
                if (currentDistance < closestDistance) {
                    closestDistance = currentDistance;
                    closestCollider = ladderCollider;
                }

                // Prioritize ropes over ladders due to the climbing animation.
                if (currentDistance == closestDistance) {
                    if (ladderCollider.CompareTag("Rope")) {
                        closestCollider = ladderCollider;
                    }
                }
            }

            return closestCollider;
        }
    }

}
