using UnityEngine;

namespace Spelunky {

    /// <summary>
    /// The basic spider enemy.
    ///
    /// Always starts hanging from the ceiling and is activated when the player walks beneath it.
    ///
    /// TODO: This entire class should be refactored. We need to generalize the state machine so that we cna use states
    /// for enemies.
    /// </summary>
    public class Spider : Entity {

        public float minJumpWaitTime;
        public float maxJumpWaitTime;
        public Vector2 jumpVelocity;
        public int damage;

        public float targetDetectionDistance;
        public LayerMask targetDetectionMask;

        public LayerMask colliderToHangFromLayerMask;
        public Collider2D colliderToHangFrom;

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

        public override void Awake() {
            base.Awake();
            Physics.OnCollisionEnterEvent.AddListener(OnEntityPhysicsCollisionEnter);
        }

        private void Start() {
            // TODO: Should get this based on the tile we're in. Also this can probably be assigned by the level
            // generator when it generates the level as it will procedurally place the spiders and only in suitable
            // locations so it will know the tile to place the spider under when it spawns the spider.
            Vector2 direction = Vector2.up;
            float rayLength = 24f;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, rayLength, colliderToHangFromLayerMask);
            Debug.DrawRay(transform.position, direction * rayLength, Color.cyan);
            colliderToHangFrom = hit.collider;
        }

        private void Update() {
            if (colliderToHangFrom == null && !_landed) {
                DetectTargetGlobal();
            }

            if (_targetToMoveTowards == null) {
                DetectTargetWhenHanging();
                return;
            }

            _velocity.y += PhysicsManager.gravity.y * Time.deltaTime;
            Physics.Move(_velocity * Time.deltaTime);

            _idleDuration -= Time.deltaTime;

            if (!_landed) {
                return;
            }

            JumpTowardsTarget();
        }

        /// <summary>
        /// TODO: Lots of stupid code here. If the block we're hanging from is destroyed we fall and in Spelunky the
        /// spiders then seemingly autotarget the player no matter the distance or line of sight. This is just a
        /// ridiculous way to reproduce that same behavior. I'm pretty sure I don't want that behavior though. If the
        /// spider falls from a block it should just jump around randomly until it spots the player in some way. Which
        /// means we need another "roaming" state for the spiders as well in addition to states we already have in here.
        /// </summary>
        private void DetectTargetGlobal() {
            float radius = LevelGenerator.instance.LevelWidth > LevelGenerator.instance.LevelHeight ? LevelGenerator.instance.LevelWidth : LevelGenerator.instance.LevelHeight;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, radius, targetDetectionMask);
            foreach (Collider2D collider in colliders) {
                _targetToMoveTowards = collider.transform;
                break;
            }

            Visuals.animator.Play("Flip");
            _flipping = true;
        }

        private void DetectTargetWhenHanging() {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, targetDetectionDistance, targetDetectionMask);
            Debug.DrawRay(transform.position, Vector2.down * targetDetectionDistance, Color.green);

            if (hit.collider != null) {
                _targetToMoveTowards = hit.transform;
                Visuals.animator.Play("Flip");
                _flipping = true;
            }
        }

        private void JumpTowardsTarget() {
            if (!Physics.collisionInfo.down) {
                if (_velocity.y > 0) {
                    Visuals.animator.Play("Jump", 1, false);
                }
                else {
                    Visuals.animator.Play("Fall", 1, false);
                }
            }
            else {
                _velocity = Vector2.zero;
                Visuals.animator.Play("Idle");
                if (_idleDuration <= 0f) {
                    DoSingleJump();
                }
            }
        }

        private void DoSingleJump() {
            float sign = Mathf.Sign(_targetToMoveTowards.position.x - transform.position.x);
            _velocity = new Vector2(jumpVelocity.x * sign, jumpVelocity.y);
        }


        private void OnEntityPhysicsCollisionEnter(CollisionInfo collisionInfo) {
            if (collisionInfo.up) {
                _velocity.y = 0;
            }

            if (collisionInfo.left || collisionInfo.right) {
                _velocity.x *= -0.25f;
            }

            if (collisionInfo.becameGroundedThisFrame) {
                _idleDuration = Random.Range(minJumpWaitTime, maxJumpWaitTime);

                if (_flipping) {
                    _flipping = false;
                    _landed = true;
                }
            }
        }

    }

}
