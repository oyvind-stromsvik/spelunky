using System.Collections;
using UnityEngine;

namespace Spelunky {

    [RequireComponent(typeof(EntityPhysics))]
    public class Bomb : MonoBehaviour {

        public EntityPhysics Physics { get; private set; }

        public Explosion explosion;

        public AudioClip bounceClip;
        public AudioClip bombTimerClip;

        public float timeToExplode;

        private AudioSource _audioSource;

        private SpriteAnimator _spriteAnimator;

        private const float BounceSoundVelocityThreshold = 100f;

        private void Awake() {
            _audioSource = GetComponent<AudioSource>();
            _spriteAnimator = GetComponent<SpriteAnimator>();
            Physics = GetComponent<EntityPhysics>();

            Physics.OnCollisionEvent.AddListener(OnCollision);
        }

        private void Start() {
            StartCoroutine(DelayedExplosion());
        }

        private IEnumerator DelayedExplosion() {
            yield return new WaitForSeconds(timeToExplode - bombTimerClip.length);

            _spriteAnimator.Play("BombArmed");

            _audioSource.clip = bombTimerClip;
            _audioSource.Play();

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

        private void OnCollision(CollisionInfo collisionInfo) {
            bool playSound = false;

            if (collisionInfo.left || collisionInfo.right) {
                if (Mathf.Abs(Physics.velocity.x) > BounceSoundVelocityThreshold) {
                    playSound = true;
                }

                Physics.velocity.x *= -1f;
            }

            if (collisionInfo.up || collisionInfo.down) {
                if (Mathf.Abs(Physics.velocity.y) > BounceSoundVelocityThreshold) {
                    playSound = true;
                }

                Physics.velocity.y *= -1f;
            }

            Physics.velocity *= 0.5f;

            if (playSound) {
                _audioSource.PlayOneShot(bounceClip);
            }
        }
    }

}
