using UnityEngine;

namespace Spelunky {
    public class SplatState : State {

        public AudioClip splatClip;

        public override void Enter() {
            player.graphics.animator.looping = false;
            player.graphics.animator.fps = 36;
            player.graphics.animator.Play("Splat", true);
            player.audio.Play(splatClip);
            player.physicsObject.Collider.enabled = false;
        }

        public override bool LockInput() {
            return true;
        }
    }
}
