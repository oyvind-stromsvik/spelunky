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
            // Horizontal movement is handled by push-on-collision.
            velocity.x = 0f;

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
            if (Mathf.Approximately(pushSpeed, 0f)) {
                return;
            }

            Physics.Move(new Vector2(pushSpeed * Time.deltaTime, 0f));
        }

    }

}
