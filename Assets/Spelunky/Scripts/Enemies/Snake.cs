using UnityEngine;

namespace Spelunky {

    public class Snake : Entity {

        public float moveSpeed;
        public int damage;

        public Vector2 _velocity;

        private void Reset() {
            moveSpeed = 16f;
            damage = 1;
        }

        public override void Awake() {
            base.Awake();
            
            Physics.OnCollisionEnterEvent.AddListener(OnEntityPhysicsCollisionEnter);
        }

        private void Update() {
            CheckIfWeAreOnALedge();

            // Move in the direction we're facing so that when we flip our visuals we also flip our movement direction.
            _velocity.x = moveSpeed * Visuals.facingDirection;

            if (Physics.collisionInfo.down) {
                _velocity.y = 0;
            }
            else {
                _velocity.y += PhysicsManager.gravity.y * Time.deltaTime;
            }

            Physics.Move(_velocity * Time.deltaTime);
        }

        private void CheckIfWeAreOnALedge() {
            Vector3 offsetForward = new Vector3(Physics.Collider.size.x * Visuals.facingDirection / 2f, 1, 0);
            RaycastHit2D hitForward = Physics2D.Raycast(transform.position + offsetForward, Vector2.down, 2, Physics.collisionMask);
            Debug.DrawRay(transform.position + offsetForward, Vector2.down * 2, Color.green);

            // Flip our character if we're on a ledge.
            if (Physics.collisionInfo.down && hitForward.collider == null) {
                Visuals.FlipCharacter();
            }
        }
        
        private void OnEntityPhysicsCollisionEnter(CollisionInfo collisionInfo) {
            if (collisionInfo.left || collisionInfo.right) {
                if (collisionInfo.colliderHorizontal.CompareTag("Player")) {
                    Attack(collisionInfo.colliderHorizontal);
                }
                else {
                    Visuals.FlipCharacter();
                }
            }
        }

        private void Attack(Collider2D colliderToAttack) {
            Visuals.animator.PlayOnceUninterrupted("Attack");
            Vector2 bombVelocity = new Vector2(256 * Visuals.facingDirection, 512);
            colliderToAttack.GetComponent<Player>().velocity = bombVelocity;
            colliderToAttack.GetComponent<EntityHealth>().TakeDamage(damage);
        }

    }

}
