using UnityEngine;

namespace Spelunky {
    [RequireComponent (typeof (PhysicsObject))]
    public class Block : MonoBehaviour, IObjectController {

        public AudioClip landClip;

        private Vector3 _velocity;

        public float pushSpeed = 0f;

        // References.
        private AudioSource _audioSource;
        public PhysicsObject physicsObject;

        private void Awake() {
            _audioSource = GetComponent<AudioSource>();
            physicsObject = GetComponent<PhysicsObject>();
        }

        // TODO: This method should be a callback from the physics object.
        private void HandleCollisions() {
            if (physicsObject.collisionInfo.becameGroundedThisFrame) {
                _audioSource.clip = landClip;
                _audioSource.Play();
            }
        }

        private void Update() {
            CalculateVelocity();

            HandleCollisions();

            physicsObject.Move(_velocity * Time.deltaTime);

            if (physicsObject.collisionInfo.down) {
                _velocity.y = 0;
            }
        }

        // TODO: This method should be a callback from the physics object.
        private void CalculateVelocity() {
            _velocity.x = pushSpeed;
            // If we're not grounded we snap our x position to the tile grid to avoid
            // floating point inaccuraies in our alignment and we zero out our
            // x velocity to avoid any further movement on the horizontal axis.
            if (!physicsObject.collisionInfo.down) {
                Vector3 centerOfBlock = transform.position + (Vector3) physicsObject.Collider.offset;
                Vector3 lowerLeftCornerOfTileWeAreIn = Tile.GetPositionOfLowerLeftOfNearestTile(centerOfBlock);
                transform.position = new Vector3(lowerLeftCornerOfTileWeAreIn.x, transform.position.y, transform.position.z);
                _velocity.x = 0;
            }
            _velocity.y += PhysicsManager.gravity.y * Time.deltaTime;
        }

        public bool IgnoreCollider(Collider2D collider, CollisionDirection direction) {
            // TODO: This squashes the player if he pushes a block and immedaitely climbs up a ladder.
            // TODO: Having logic like this in this callback just seems wonky.
            if (!physicsObject.collisionInfo.down && direction == CollisionDirection.Down && collider.CompareTag("Player")) {
                Player player = collider.GetComponent<Player>();
                player.Splat();
                return true;
            }

            return false;
        }

        public void OnCollision(CollisionInfo collisionInfo) {
        }

        public void UpdateVelocity(ref Vector2 velocity) {

        }
    }
}
