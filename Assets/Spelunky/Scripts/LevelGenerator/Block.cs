using UnityEngine;

namespace Spelunky {
    [RequireComponent (typeof (PhysicsObject))]
    public class Block : MonoBehaviour {

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

        private void HandleCollisions() {
            if (physicsObject.collisions.becameGroundedThisFrame) {
                _audioSource.clip = landClip;
                _audioSource.Play();
            }
        }

        private void Update() {
            CalculateVelocity();

            HandleCollisions();

            physicsObject.Move(_velocity * Time.deltaTime);

            if (physicsObject.collisions.below) {
                _velocity.y = 0;
            }
        }

        private void CalculateVelocity() {
            _velocity.x = pushSpeed;
            // If we're not grounded we snap our x position to the tile grid to avoid
            // floating point inaccuraies in our alignment and we zero out our
            // x velocity to avoid any further movement on the horizontal axis.
            if (!physicsObject.collisions.below) {
                Vector3 centerOfBlock = transform.position + (Vector3) physicsObject.collider.offset;
                Vector3 lowerLeftCornerOfTileWeAreIn = Tile.GetPositionOfLowerLeftOfNearestTile(centerOfBlock);
                transform.position = new Vector3(lowerLeftCornerOfTileWeAreIn.x, transform.position.y, transform.position.z);
                _velocity.x = 0;
            }
            _velocity.y += PhysicsManager.gravity.y * Time.deltaTime;
        }
    }
}
