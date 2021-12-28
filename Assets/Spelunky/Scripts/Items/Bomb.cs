using System.Collections;
using UnityEngine;

namespace Spelunky {

    public class Bomb : Entity {

        public Explosion explosion;

        public AudioClip bounceClip;
        public AudioClip bombTimerClip;

        public float timeToExplode;

        private AudioSource _audioSource;

        public Vector2 _velocity;

        private const float BounceSoundVelocityThreshold = 100f;

        public override void Awake() {
            base.Awake();

            _audioSource = GetComponent<AudioSource>();

            Physics.OnCollisionEnterEvent.AddListener(OnEntityPhysicsCollisionEnter);
        }

        private void Start() {
            StartCoroutine(DelayedExplosion());
        }

        private void Update() {
            _velocity.y += PhysicsManager.gravity.y * Time.deltaTime;

            if (Physics.collisionInfo.down) {
                _velocity *= 0.9f;
            }

            Physics.Move(_velocity * Time.deltaTime);
        }

        private IEnumerator DelayedExplosion() {
            yield return new WaitForSeconds(timeToExplode - bombTimerClip.length);

            Visuals.animator.Play("BombArmed");

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

        private void OnEntityPhysicsCollisionEnter(CollisionInfo collisionInfo) {
            bool playSound = false;

            if (collisionInfo.left || collisionInfo.right) {
                if (Mathf.Abs(_velocity.x) > BounceSoundVelocityThreshold) {
                    playSound = true;
                }

                _velocity.x *= -1f;
            }

            if (collisionInfo.up || collisionInfo.down) {
                if (Mathf.Abs(_velocity.y) > BounceSoundVelocityThreshold) {
                    playSound = true;
                }

                _velocity.y *= -1f;
            }

            _velocity *= 0.5f;

            if (playSound) {
                _audioSource.PlayOneShot(bounceClip);
            }
        }
    }

}
