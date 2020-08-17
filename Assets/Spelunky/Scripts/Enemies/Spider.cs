using UnityEngine;

namespace Spelunky {
    public class Spider : Enemy {

        public float minJumpWaitTime;
        public float maxJumpWaitTime;
        public Vector2 jumpVelocity;
        public int damage;

        public float targetDetectionDistance;
        public LayerMask targetDetectionMask;

        private Vector2 _velocity;

        private Transform _targetToMoveTowards;
        private bool _flipping;
        private bool _landed;
        private float _idleDuration;

        private void Reset() {
            minJumpWaitTime = 1f;
            maxJumpWaitTime = 3f;
            jumpVelocity = new Vector2(96, 196);
            damage = 1;
            targetDetectionDistance = 128;
        }

        private void Update() {
            DetectTargetWhenHanging();

            if (_targetToMoveTowards == null) {
                return;
            }

            _velocity.y += PhysicsManager.gravity.y * Time.deltaTime;
            PhysicsObject.Move(_velocity * Time.deltaTime);

            _idleDuration -= Time.deltaTime;

            if (_flipping && PhysicsObject.collisionInfo.becameGroundedThisFrame) {
                _flipping = false;
                _landed = true;
            }

            if (!_landed) {
                return;
            }

            JumpTowardsTarget();
        }

        public override bool IgnoreCollider(Collider2D collider, CollisionDirection direction) {
            if (collider.CompareTag("Player")) {
                collider.GetComponent<Player>().TakeDamage(damage, direction);
                return true;
            }

            return false;
        }


        private void DetectTargetWhenHanging() {
            if (_targetToMoveTowards) {
                return;
            }

            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, targetDetectionDistance, targetDetectionMask);
            Debug.DrawRay(transform.position, Vector2.down * targetDetectionDistance, Color.green);

            if (hit.collider != null) {
                _targetToMoveTowards = hit.transform;
                EntityVisuals.animator.fps = 24;
                EntityVisuals.animator.Play("Flip", true);
                _flipping = true;
            }
        }

        private void JumpTowardsTarget() {
            if (PhysicsObject.collisionInfo.becameGroundedThisFrame) {
                _idleDuration = Random.Range(minJumpWaitTime, maxJumpWaitTime);
            }

            if (PhysicsObject.collisionInfo.up) {
                _velocity.y = 0;
            }

            if (PhysicsObject.collisionInfo.left || PhysicsObject.collisionInfo.right) {
                _velocity.x *= -0.25f;
            }

            if (!PhysicsObject.collisionInfo.down) {
                EntityVisuals.animator.looping = false;
                if (_velocity.y > 0) {
                    EntityVisuals.animator.Play("Jump", true);
                }
                else {
                    EntityVisuals.animator.Play("Fall", true);
                }
            }
            else {
                _velocity = Vector2.zero;
                EntityVisuals.animator.fps = 12;
                EntityVisuals.animator.looping = true;
                EntityVisuals.animator.Play("Idle");
                if (_idleDuration <= 0f) {
                    DoSingleJump();
                }
            }
        }

        private void DoSingleJump() {
            float sign = Mathf.Sign(_targetToMoveTowards.position.x - transform.position.x);
            _velocity = new Vector2(jumpVelocity.x * sign, jumpVelocity.y);
        }

    }
}
