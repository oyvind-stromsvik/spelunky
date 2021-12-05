using UnityEngine;

namespace Spelunky {

    public class SplatState : State {
        public AudioClip splatClip;

        public override void Enter() {
            player.Visuals.animator.Play("Splat");
            player.Audio.Play(splatClip);
            player.Physics.Collider.enabled = false;
        }

        public override bool LockInput() {
            return true;
        }
    }

}
