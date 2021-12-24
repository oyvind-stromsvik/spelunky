using UnityEngine;

namespace Spelunky {

    [RequireComponent(typeof(EntityPhysics))]
    public class Block : MonoBehaviour {
        public AudioClip landClip;

        private Vector3 _velocity;

        public float pushSpeed = 0f;

        // References.
        private AudioSource _audioSource;
        public EntityPhysics entityPhysics;

        private void Awake() {
            _audioSource = GetComponent<AudioSource>();
            entityPhysics = GetComponent<EntityPhysics>();
            entityPhysics.OnCollisionEvent.AddListener(OnCollision);
        }

        private void Update() {
            CalculateVelocity();

            entityPhysics.Move(_velocity * Time.deltaTime);

            if (entityPhysics.collisionInfo.down) {
                _velocity.y = 0;
            }
        }

        // TODO: This method should be a callback from the physics object.
        private void CalculateVelocity() {
            _velocity.x = pushSpeed;
            // If we're not grounded we snap our x position to the tile grid to avoid
            // floating point inaccuraies in our alignment and we zero out our
            // x velocity to avoid any further movement on the horizontal axis.
            if (!entityPhysics.collisionInfo.down) {
                Vector3 centerOfBlock = transform.position + (Vector3) entityPhysics.Collider.offset;
                Vector3 lowerLeftCornerOfTileWeAreIn = Tile.GetPositionOfLowerLeftOfNearestTile(centerOfBlock);
                transform.position = new Vector3(lowerLeftCornerOfTileWeAreIn.x, transform.position.y, transform.position.z);
                _velocity.x = 0;
            }

            _velocity.y += PhysicsManager.gravity.y * Time.deltaTime;
        }

        public void OnCollision(CollisionInfo collisionInfo) {
            if (collisionInfo.down && collisionInfo.collider.CompareTag("Player")) {
                Player player = collisionInfo.collider.GetComponent<Player>();
                player.Splat();
            }

            if (collisionInfo.becameGroundedThisFrame) {
                _audioSource.clip = landClip;
                _audioSource.Play();
            }
        }

        public void Push(float pushSpeed) {
            print(pushSpeed);
            this.pushSpeed = pushSpeed;
        }
    }

}
