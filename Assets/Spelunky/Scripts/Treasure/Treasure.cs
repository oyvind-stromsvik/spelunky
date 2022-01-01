using UnityEngine;

namespace Spelunky {

    /// <summary>
    /// TODO: Almost identical to the bomb and I imagine every "rigidbody" will have similar logic. There should be a
    /// generic rigidbody class. Should that be part of EntityPhysics or what? We need a dedicated character controller
    /// class and maybe entity physics can behave like the rigidbody which is the base for both a character controller
    /// and things like this?
    /// </summary>
    public class Treasure : Entity {

        public AudioClip bounceClip;
        public AudioClip pickUpSound;
        public int value;

        private AudioSource _audioSource;

        public Vector2 _velocity;

        private const float BounceSoundVelocityThreshold = 100f;

        public override void Awake() {
            base.Awake();

            _audioSource = GetComponent<AudioSource>();

            Physics.OnCollisionEnterEvent.AddListener(OnEntityPhysicsCollisionEnter);
        }

        private void Update() {
            if (Physics.collisionInfo.down) {
                _velocity.y = 0;
            }
            else {
                _velocity.y += PhysicsManager.gravity.y * Time.deltaTime;
            }

            if (Physics.collisionInfo.down) {
                _velocity *= 0.9f;
            }

            Physics.Move(_velocity * Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other) {
            Player player = other.GetComponent<Player>();
            if (player != null) {
                player.Inventory.PickupGold(value);
                AudioManager.Instance.PlaySoundAtPosition(pickUpSound, transform.position, AudioManager.AudioGroup.SFX);
                Destroy(gameObject);
            }
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

            // TODO: Play a bounce sound.
            if (playSound) {
                //_audioSource.PlayOneShot(bounceClip);
            }
        }

    }

}
