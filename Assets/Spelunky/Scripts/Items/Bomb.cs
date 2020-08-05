using System.Collections;
using UnityEngine;

namespace Spelunky {
    [RequireComponent (typeof (PhysicsObject))]
    public class Bomb : MonoBehaviour {

        public Explosion explosion;

        public AudioClip bounceClip;
        public AudioClip bombTimerClip;

        public float timeToExplode;

        private AudioSource _audioSource;

        private Vector3 _offset;

        private Vector3 _velocity;

        private PhysicsObject _controller;
        private SpriteAnimator _spriteAnimator;

        private const float BounceSoundVelocityThreshold = 100f;

        private void Awake() {
            _controller = GetComponent<PhysicsObject>();
            _audioSource = GetComponent<AudioSource>();
            _spriteAnimator = GetComponent<SpriteAnimator>();
        }

        private void Start() {
            StartCoroutine(DelayedExplosion());
            _offset = new Vector3(0, 4, 0);
        }

        private void Update() {
            CalculateVelocity();

            HandleCollisions();

            _controller.Move(_velocity * Time.deltaTime);
        }

        public void SetVelocity(Vector2 velocity) {
            _velocity = velocity;
        }

        private void HandleCollisions() {
            if (_controller.collisions.collidedThisFrame && !_controller.collisions.collidedLastFrame) {
                bool playSound = false;

                if (_controller.collisions.right || _controller.collisions.left) {
                    if (Mathf.Abs(_velocity.x) > BounceSoundVelocityThreshold) {
                        playSound = true;
                    }
                    _velocity.x *= -1f;
                }

                if (_controller.collisions.above || _controller.collisions.below) {
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

        private void CalculateVelocity() {
            _velocity.y += PhysicsManager.gravity.y * Time.deltaTime;
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
            Instantiate(explosion, transform.position + _offset, Quaternion.identity);
            Destroy(gameObject);
        }

    }
}
