using System.Collections;
using UnityEngine;

namespace Spelunky {

    /// <summary>
    ///
    /// </summary>
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
            if (collisionInfo.left || collisionInfo.right) {
                _velocity.x *= -1f;
            }

            if (collisionInfo.up || collisionInfo.down) {
                _velocity.y *= -1f;
            }

            _velocity *= 0.5f;

            if (Physics.Velocity.magnitude > BounceSoundVelocityThreshold) {
                _audioSource.PlayOneShot(bounceClip);
            }
        }

    }

}
