using UnityEngine;

namespace Spelunky {

    public class Block : Entity {
        public AudioClip landClip;

        private Vector3 _velocity;

        // References.
        private AudioSource _audioSource;

        public override void Awake() {
            base.Awake();
            _audioSource = GetComponent<AudioSource>();
            Physics.OnCollisionEnterEvent.AddListener(OnEntityPhysicsCollisionEnter);
        }

        private void Update() {
            // If we're not grounded we snap our x position to the tile grid to avoid
            // floating point inaccuraies in our alignment and we zero out our
            // x velocity to avoid any further movement on the horizontal axis.
            if (!Physics.collisionInfo.down) {
                Vector3 centerOfBlock = transform.position + (Vector3) Physics.Collider.offset;
                Vector3 lowerLeftCornerOfTileWeAreIn = Tile.GetPositionOfLowerLeftOfNearestTile(centerOfBlock);
                transform.position = new Vector3(lowerLeftCornerOfTileWeAreIn.x, transform.position.y, transform.position.z);
                _velocity.x = 0;
            }

            _velocity.y += PhysicsManager.gravity.y * Time.deltaTime;

            Physics.Move(_velocity * Time.deltaTime);

            if (Physics.collisionInfo.down) {
                _velocity.y = 0;
            }
        }

        private void OnEntityPhysicsCollisionEnter(CollisionInfo collisionInfo) {
            if (collisionInfo.down && collisionInfo.colliderVertical.CompareTag("Player")) {
                Player player = collisionInfo.colliderVertical.GetComponent<Player>();
                player.Splat();
            }

            if (collisionInfo.becameGroundedThisFrame) {
                _audioSource.clip = landClip;
                _audioSource.Play();
            }

            if (collisionInfo.down) {
                _velocity.y = 0;
            }
        }

        public void Push(float pushSpeed) {
            _velocity.x = pushSpeed;
        }
    }

}
