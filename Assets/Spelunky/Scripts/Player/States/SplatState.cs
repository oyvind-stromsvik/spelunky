using UnityEngine;

namespace Spelunky {

    /// <summary>
    /// Currently the state for when we get squished by a falling block. We'll see if this becomes a generic dead state
    /// or not.
    ///
    /// TODO: Implement dying to spikes and a generic death and see how it fits.
    /// </summary>
    public class SplatState : State {

        public AudioClip splatClip;

        public override void EnterState() {
            player.Visuals.animator.Play("Splat");
            player.Audio.Play(splatClip);
            // TODO: This is temporary. The health class should handle dying and death effects. We're not technically
            // dead here currently either as nothing has reduced our health.
            Instantiate(player.Health.bloodParticles, transform.position, Quaternion.identity);
            player.Physics.Collider.enabled = false;
        }

        public override void ChangePlayerVelocity(ref Vector2 velocity) {
            velocity = Vector2.zero;
        }

        public override bool LockInput() {
            return true;
        }

    }

}
