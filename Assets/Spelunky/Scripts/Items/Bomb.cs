using System.Collections;
using UnityEngine;

namespace Spelunky {
    [RequireComponent (typeof (PhysicsObject))]
    public class Bomb : MonoBehaviour, IObjectController {

        public Explosion explosion;

        public AudioClip bounceClip;
        public AudioClip bombTimerClip;

        public float timeToExplode;

        private AudioSource _audioSource;

        private Vector3 _velocity;

        private SpriteAnimator _spriteAnimator;

        private const float BounceSoundVelocityThreshold = 100f;

        private void Awake() {
            _audioSource = GetComponent<AudioSource>();
            _spriteAnimator = GetComponent<SpriteAnimator>();
        }

        private void Start() {
            StartCoroutine(DelayedExplosion());
        }

        public void SetVelocity(Vector2 velocity) {
            _velocity = velocity;
        }

        private IEnumerator DelayedExplosion() {
            _spriteAnimator.fps = 0;

            yield return new WaitForSeconds(timeToExplode - bombTimerClip.length);

            _spriteAnimator.Play("BombArmed");
            _spriteAnimator.fps = 24;

            _audioSource.clip = bombTimerClip;
            _audioSource.Play();

            yield return new WaitForSeconds(bombTimerClip.length);

            Explode();
        }

        public void Explode() {
            Instantiate(explosion, transform.position + new Vector3(0, 4, 0), Quaternion.identity);
            Destroy(gameObject);
        }

        public bool IgnoreCollider(Collider2D collider, CollisionDirection direction) {
            return false;
        }

        public void OnCollision(CollisionInfo collisionInfo) {
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

        public void UpdateVelocity(ref Vector2 velocity) {
            _velocity.y += PhysicsManager.gravity.y * Time.deltaTime;
            velocity = _velocity;
        }
    }
}
