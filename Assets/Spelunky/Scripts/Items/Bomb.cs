using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spelunky {
    [RequireComponent (typeof (PhysicsObject))]
    public class Bomb : MonoBehaviour {

        public GameObject explosion;

        public AudioClip bounceClip;
        public AudioClip bombTimerClip;
        public AudioClip bombExplosionClip;

        public float timeToExplode;
        public float explotionRadius;
        public LayerMask layerMask;

        private AudioSource _audioSource;

        private Vector3 _offset;

        public float maxJumpHeight;
        public float timeToJumpApex;

        private Vector3 _velocity;

        private PhysicsObject _controller;
        private SpriteAnimator _spriteAnimator;

        private float _boundsSoundVelocityThreshold = 100f;
        private float _sleepVelocityThreshold = 35f;
        private bool _sleep;

        public void SetVelocity(Vector2 velocity) {
            _velocity = velocity;
        }

        private void HandleCollisions() {
            if (_controller.collisions.collidedThisFrame && !_controller.collisions.collidedLastFrame) {
                bool playSound = false;

                if (_controller.collisions.right || _controller.collisions.left) {
                    if (Mathf.Abs(_velocity.x) > _boundsSoundVelocityThreshold) {
                        playSound = true;
                    }
                    _velocity.x *= -1f;
                }

                if (_controller.collisions.above || _controller.collisions.below) {
                    if (Mathf.Abs(_velocity.y) > _boundsSoundVelocityThreshold) {
                        playSound = true;
                    }
                    _velocity.y *= -1f;
                }

                _velocity *= 0.5f;

                if (_velocity.magnitude < _sleepVelocityThreshold && _controller.collisions.below) {
                    _sleep = true;
                }

                if (playSound) {
                    _audioSource.clip = bounceClip;
                    _audioSource.Play();
                }
            }
        }

        private void Awake() {
            _controller = GetComponent<PhysicsObject>();
            _audioSource = GetComponent<AudioSource>();
            _spriteAnimator = GetComponent<SpriteAnimator>();
        }

        private void Start() {
            StartCoroutine(Explode());
            _offset = new Vector3(0, 4, 0);
        }

        private IEnumerator Explode() {
            _spriteAnimator.fps = 0;

            yield return new WaitForSeconds(timeToExplode - bombTimerClip.length);

            _spriteAnimator.Play("BombArmed");
            _spriteAnimator.fps = 24;

            _audioSource.clip = bombTimerClip;
            _audioSource.Play();

            yield return new WaitForSeconds(bombTimerClip.length);

            _audioSource.clip = bombExplosionClip;
            _audioSource.Play();

            Instantiate(explosion, transform.position + _offset, Quaternion.identity);

            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position + _offset, explotionRadius, layerMask);
            List<Tile> tilesToRemove = new List<Tile>();
            foreach (Collider2D collider in colliders) {
                Tile tile = collider.GetComponent<Tile>();
                if (tile != null) {
                    tilesToRemove.Add(tile);
                }
            }

            LevelGenerator.instance.RemoveTiles(tilesToRemove.ToArray());

            GetComponent<SpriteRenderer>().enabled = false;
            GetComponent<PhysicsObject>().enabled = false;
            GetComponent<Bomb>().enabled = false;
            Destroy(gameObject, 2f);
        }

        private void OnDrawGizmos() {
            Gizmos.DrawWireSphere(transform.position + _offset, explotionRadius);
        }

        private void Update() {
            CalculateVelocity();

            HandleCollisions();

            if (_sleep) {
                _velocity = Vector2.zero;
            }

            _controller.Move(_velocity * Time.deltaTime);
        }

        private void CalculateVelocity() {
            _velocity.y += PhysicsManager.gravity.y * Time.deltaTime;
        }
    }
}
