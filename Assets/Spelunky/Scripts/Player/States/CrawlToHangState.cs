using System.Collections;
using UnityEngine;

// TODO: This is actually a transition between grounded and hanging.
//   Figure out a way how to solve this more elegantly?
namespace Spelunky {

    public class CrawlToHangState : State {
        public override void Enter() {
            StartCoroutine(CrawlToHang());
        }

        private IEnumerator CrawlToHang() {
            // Find the collider we're going to grab on to.
            Vector3 offset = new Vector3(-6 * player.Visuals.facingDirection, 1, 0);
            RaycastHit2D hit = Physics2D.Raycast(transform.position + offset, Vector2.down, 2, player.edgeGrabLayerMask);
            Debug.DrawRay(transform.position + offset, Vector2.down * 4, Color.yellow);

            player.Visuals.animator.Play("CrawlToHang", true);
            player.Visuals.animator.fps = 24;

            yield return new WaitForSeconds(player.Visuals.animator.GetAnimationLength("CrawlToHang"));

            player.hangingState.colliderToHangFrom = hit.collider;
            player.stateMachine.AttemptToChangeState(player.hangingState);
        }

        public override void ChangePlayerVelocity(ref Vector2 velocity) {
            velocity = Vector2.zero;
        }
    }

}