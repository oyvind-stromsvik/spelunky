﻿using UnityEngine;

namespace Spelunky {

    public class Spider : Entity {
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

        public override void Awake() {
            base.Awake();
            Physics.OnCollisionEnterEvent.AddListener(OnEntityPhysicsCollisionEnter);
        }

        private void Update() {
            DetectTargetWhenHanging();

            if (_targetToMoveTowards == null) {
                return;
            }

            _velocity.y += PhysicsManager.gravity.y * Time.deltaTime;
            Physics.Move(_velocity * Time.deltaTime);

            _idleDuration -= Time.deltaTime;

            if (_flipping && Physics.collisionInfo.becameGroundedThisFrame) {
                _flipping = false;
                _landed = true;
            }

            if (!_landed) {
                return;
            }

            JumpTowardsTarget();
        }

        private void DetectTargetWhenHanging() {
            if (_targetToMoveTowards) {
                return;
            }

            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, targetDetectionDistance, targetDetectionMask);
            Debug.DrawRay(transform.position, Vector2.down * targetDetectionDistance, Color.green);

            if (hit.collider != null) {
                _targetToMoveTowards = hit.transform;
                Visuals.animator.Play("Flip");
                _flipping = true;
            }
        }

        private void JumpTowardsTarget() {
            if (Physics.collisionInfo.becameGroundedThisFrame) {
                _idleDuration = Random.Range(minJumpWaitTime, maxJumpWaitTime);
            }

            if (!Physics.collisionInfo.down) {
                if (_velocity.y > 0) {
                    Visuals.animator.Play("Jump");
                }
                else {
                    Visuals.animator.Play("Fall");
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
        }

    }

}
