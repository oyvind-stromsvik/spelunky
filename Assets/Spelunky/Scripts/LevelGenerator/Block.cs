using UnityEngine;

namespace Spelunky {

    /// <summary>
    /// A pushable block.
    /// </summary>
    public class Block : Entity {

        public AudioClip landClip;

        private Vector3 _velocity;

        // References.
        private AudioSource _audioSource;
        private bool _initialized;

        public override void Awake() {
            base.Awake();
            _audioSource = GetComponent<AudioSource>();
            Physics.OnCollisionEnterEvent.AddListener(OnEntityPhysicsCollisionEnter);
        }

        private void Update() {
            // If we're not grounded we snap our x position to the tile grid to avoid floating point inaccuraies in our
            // alignment and we zero out our x velocity to avoid any further movement on the horizontal axis.
            if (!Physics.collisionInfo.down) {
                Vector3 centerOfBlock = transform.position + (Vector3) Physics.Collider.offset;
                Vector3 lowerLeftCornerOfTileWeAreIn = Tile.GetPositionOfLowerLeftOfNearestTile(centerOfBlock);
                transform.position = new Vector3(lowerLeftCornerOfTileWeAreIn.x, transform.position.y, transform.position.z);
                _velocity.x = 0;
            }

            if (Physics.collisionInfo.down) {
                _velocity.y = 0;
            }
            else {
                _velocity.y += PhysicsManager.gravity.y * Time.deltaTime;
            }

            Physics.Move(_velocity * Time.deltaTime);
        }

        private void OnEntityPhysicsCollisionEnter(CollisionInfo collisionInfo) {
            // Not sure if hacky or not, but I added this to avoid the landClip from playing on level start for all
            // blocks in the level.
            if (!_initialized) {
                _initialized = true;
                return;
            }

            if (collisionInfo.becameGroundedThisFrame) {
                if (collisionInfo.colliderVertical.CompareTag("Player")) {
                    Player player = collisionInfo.colliderVertical.GetComponent<Player>();
                    player.Splat();
                }
                else {
                    _audioSource.clip = landClip;
                    _audioSource.Play();
                }
            }
        }

        public void Push(float pushSpeed) {
            _velocity.x = pushSpeed;
        }

    }

}
