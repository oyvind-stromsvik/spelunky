using System.Collections;
using UnityEngine;

namespace Spelunky {

    /// <summary>
    /// This state is essentially just a transition to the HangingState, but you can only enter this state by crawling
    /// and you can only enter the HangingState by jumping so they behave very differently.
    /// </summary>
    public class CrawlToHangState : State {

        public override bool CanEnterState() {
            // Find the collider we're going to grab on to.
            Vector3 offset = new Vector3(-6 * player.Visuals.facingDirection, 1, 0);
            RaycastHit2D hit = Physics2D.Raycast(transform.position + offset, Vector2.down, 2, player.edgeGrabLayerMask);
            if (hit.collider == null) {
                return false;
            }

            player.hangingState.colliderToHangFrom = hit.collider;

            return true;
        }

        public override void EnterState() {
            StartCoroutine(CrawlToHang());
        }

        public override void ChangePlayerVelocity(ref Vector2 velocity) {
            velocity = Vector2.zero;
        }

        private IEnumerator CrawlToHang() {
            player.Visuals.animator.Play("CrawlToHang");

            yield return new WaitForSeconds(player.Visuals.animator.GetAnimationLength("CrawlToHang"));

            player.stateMachine.AttemptToChangeState(player.hangingState);
        }

    }

}
