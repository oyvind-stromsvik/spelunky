using UnityEngine;

namespace Spelunky {

    /// <summary>
    /// A pushable block that can crush the player.
    /// </summary>
    public class Block : PhysicsBody {

        public AudioClip landClip;

        private bool _initialized;

        public override void Awake() {
            base.Awake();

            // Blocks don't bounce or have friction.
            bounces = false;
            applyFriction = false;
            stopOnGround = true;
        }

        protected override void Update() {
            // If we're not grounded we snap our x position to the tile grid to avoid floating point
            // inaccuracies in our alignment and we zero out our x velocity.
            if (!Physics.collisionInfo.down) {
                Vector3 centerOfBlock = transform.position + (Vector3) Physics.Collider.offset;
                Vector3 lowerLeftCornerOfTileWeAreIn = Tile.GetPositionOfLowerLeftOfNearestTile(centerOfBlock);
                transform.position = new Vector3(lowerLeftCornerOfTileWeAreIn.x, transform.position.y, transform.position.z);
                velocity.x = 0;
            }

            base.Update();
        }

        protected override void OnPhysicsCollisionEnter(CollisionInfo collisionInfo) {
            // Skip first collision to avoid landClip playing on level start for all blocks.
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
                    audioSource.clip = landClip;
                    audioSource.Play();
                }
            }
        }

        public void Push(float pushSpeed) {
            velocity.x = pushSpeed;
        }

    }

}
