using UnityEngine;

namespace Spelunky {
    [RequireComponent (typeof (PhysicsObject))]
    public class Block : MonoBehaviour {

        public AudioClip landClip;

        public float maxJumpHeight;
        public float timeToJumpApex;
        public float speed;

        private Vector3 _velocity;

        private float _gravity;

        // References.
        private AudioSource _audioSource;
        private PhysicsObject _controller;

        private float _boundsSoundVelocityThreshold = 30f;

        private void Awake() {
            _audioSource = GetComponent<AudioSource>();
            _controller = GetComponent<PhysicsObject>();
        }

        private void Start() {
            _gravity = -(2 * maxJumpHeight) / Mathf.Pow (timeToJumpApex, 2);
        }

        private void HandleCollisions() {
            if (_controller.collisions.collidedThisFrame && !_controller.collisions.collidedLastFrame) {
                bool playSound = false;

                if (_controller.collisions.below) {
                    if (Mathf.Abs(_velocity.y) > _boundsSoundVelocityThreshold) {
                        playSound = true;
                        _velocity.y = 0;
                    }
                }

                if (playSound) {
                    _audioSource.clip = landClip;
                    _audioSource.Play();
                }
            }
        }

        private void Update() {
            CalculateVelocity();

            HandleCollisions();

            _controller.Move(_velocity);

            if (_controller.collisions.below) {
                _velocity.y = 0;
            }
        }

        private void CalculateVelocity() {
            _velocity.x = speed;
            _velocity.y += _gravity * Time.deltaTime;
        }
    }
}
