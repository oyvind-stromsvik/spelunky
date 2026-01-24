using System.Collections;
using UnityEngine;

namespace Spelunky {

    /// <summary>
    /// A throwable bomb that explodes after a delay.
    /// </summary>
    public class Bomb : PhysicsBody, ICrushable {

        public Explosion explosion;
        public AudioClip bombTimerClip;
        public float timeToExplode;

        public override void Awake() {
            base.Awake();

            // Bomb always falls (doesn't stop on ground), but does have friction.
            stopOnGround = false;
            applyFriction = true;
            bounces = true;
        }

        private void Start() {
            StartCoroutine(DelayedExplosion());
        }

        private IEnumerator DelayedExplosion() {
            yield return new WaitForSeconds(timeToExplode - bombTimerClip.length);

            Visuals.animator.Play("BombArmed");

            audioSource.clip = bombTimerClip;
            audioSource.Play();

            yield return new WaitForSeconds(bombTimerClip.length);

            Explode();
        }

        public void Explode(float delay = 0f) {
            StartCoroutine(DoExplode(delay));
        }

        private IEnumerator DoExplode(float delay = 0f) {
            yield return new WaitForSeconds(delay);
            Instantiate(explosion, transform.position + new Vector3(0, 4, 0), Quaternion.identity);
            Destroy(gameObject);
        }

        protected override void PlayBounceSound() {
            // Bomb uses overall velocity magnitude for sound check.
            if (audioSource != null && bounceClip != null && Physics.Velocity.magnitude > bounceSoundThreshold) {
                audioSource.PlayOneShot(bounceClip);
            }
        }

        public bool IsCrushable => true;

        public void Crush() {
            Explode();
        }

    }

}
