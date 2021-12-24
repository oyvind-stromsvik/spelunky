using UnityEngine;

namespace Spelunky {

    public class Snake : Enemy {
        public float moveSpeed;
        public int damage;

        private Vector2 _velocity;

        private void Reset() {
            moveSpeed = 16f;
            damage = 1;
        }

        public override void Awake() {
            base.Awake();
            EntityPhysics.OnCollisionEvent.AddListener(OnCollision);
        }

        private void Update() {
            HandleUnsteady();

            CalculateVelocity();

            EntityPhysics.Move(_velocity * Time.deltaTime);

            if (EntityPhysics.collisionInfo.down) {
                _velocity.y = 0;
            }
        }

        public void OnCollision(CollisionInfo collisionInfo) {
            if (collisionInfo.right || collisionInfo.left) {
                EntityVisuals.FlipCharacter();
            }
        }

        private void CalculateVelocity() {
            _velocity.x = moveSpeed * EntityVisuals.facingDirection;
            _velocity.y += PhysicsManager.gravity.y * Time.deltaTime;
        }

        private void HandleUnsteady() {
            Vector3 offsetForward = new Vector3(EntityPhysics.Collider.size.x * EntityVisuals.facingDirection / 2f, 1, 0);
            RaycastHit2D hitForward = Physics2D.Raycast(transform.position + offsetForward, Vector2.down, 2, EntityPhysics.collisionMask);
            Debug.DrawRay(transform.position + offsetForward, Vector2.down * 2, Color.green);

            // Play unsteady animation
            if (EntityPhysics.collisionInfo.down && hitForward.collider == null) {
                EntityVisuals.FlipCharacter();
            }
        }
    }

}
