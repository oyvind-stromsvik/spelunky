using System.Collections.Generic;
using UnityEngine;

namespace Spelunky {

    /// <summary>
    /// Applies damage to this object when overlapping a DamageArea.
    /// </summary>
    [RequireComponent(typeof(EntityPhysics))]
    public class DamageReceiver : MonoBehaviour {

        [Tooltip("Layers that should be detected for damage overlaps.")]
        public LayerMask damageOverlapMask;

        private EntityPhysics _physics;
        private IDamageable _damageable;
        private Dictionary<DamageArea, float> _nextDamageTime = new Dictionary<DamageArea, float>();

        private void Awake() {
            _physics = GetComponent<EntityPhysics>();
            _damageable = GetComponent<IDamageable>() ?? GetComponentInParent<IDamageable>();

            if (_physics != null) {
                if (damageOverlapMask.value != 0) {
                    _physics.overlapMask |= damageOverlapMask;
                }

                _physics.OnOverlapEnterEvent.AddListener(OnOverlapEnter);
                _physics.OnOverlapExitEvent.AddListener(OnOverlapExit);
                _physics.OnOverlapStayEvent.AddListener(OnOverlapStay);
                _physics.OnCollisionEnterEvent.AddListener(OnCollisionEnter);
                _physics.OnCollisionExitEvent.AddListener(OnCollisionExit);
                _physics.OnCollisionStayEvent.AddListener(OnCollisionStay);
            }
        }

        private void OnCollisionEnter(CollisionInfo collisionInfo) {
            HandleCollisionDamage(collisionInfo, true);
        }

        private void OnCollisionStay(CollisionInfo collisionInfo) {
            HandleCollisionDamage(collisionInfo, false);
        }

        private void OnCollisionExit(CollisionInfo collisionInfo) {
            if (_damageable == null) {
                return;
            }

            RemoveDamageAreaIfExited(collisionInfo.colliderVertical);

            if (!ReferenceEquals(collisionInfo.colliderHorizontal, collisionInfo.colliderVertical)) {
                RemoveDamageAreaIfExited(collisionInfo.colliderHorizontal);
            }
        }

        private void OnOverlapEnter(Collider2D other) {
            if (_damageable == null || other == null) {
                return;
            }

            TryApplyDamage(other, true);
        }

        private void OnOverlapStay(Collider2D other) {
            if (_damageable == null || other == null) {
                return;
            }

            TryApplyDamage(other, false);
        }

        private void OnOverlapExit(Collider2D other) {
            if (other == null) {
                return;
            }

            DamageArea damageArea = other.GetComponentInParent<DamageArea>();
            if (damageArea == null) {
                return;
            }

            _nextDamageTime.Remove(damageArea);
        }

        private void HandleCollisionDamage(CollisionInfo collisionInfo, bool isEnter) {
            if (_damageable == null) {
                return;
            }

            Collider2D vertical = collisionInfo.colliderVertical;
            Collider2D horizontal = collisionInfo.colliderHorizontal;

            if ((collisionInfo.down || collisionInfo.up) && vertical != null) {
                TryApplyDamage(vertical, isEnter);
            }

            if ((collisionInfo.left || collisionInfo.right) && horizontal != null && !ReferenceEquals(horizontal, vertical)) {
                TryApplyDamage(horizontal, isEnter);
            }
        }

        private void TryApplyDamage(Collider2D other, bool isEnter) {
            DamageArea damageArea = other.GetComponentInParent<DamageArea>();
            if (damageArea == null) {
                return;
            }

            if (!damageArea.CanDamage(_physics, _physics.Collider)) {
                return;
            }

            float interval = damageArea.damageInterval;
            if (interval <= 0f) {
                if (isEnter) {
                    _damageable.TryTakeDamage(damageArea.damage);
                }
                return;
            }

            float now = Time.time;
            if (!_nextDamageTime.TryGetValue(damageArea, out float nextTime) || now >= nextTime) {
                _damageable.TryTakeDamage(damageArea.damage);
                _nextDamageTime[damageArea] = now + interval;
            }
        }

        private void RemoveDamageAreaIfExited(Collider2D collider2D) {
            if (collider2D == null) {
                return;
            }

            CollisionInfo currentCollision = _physics.collisionInfo;
            if (ReferenceEquals(currentCollision.colliderVertical, collider2D) || ReferenceEquals(currentCollision.colliderHorizontal, collider2D)) {
                return;
            }

            DamageArea damageArea = collider2D.GetComponentInParent<DamageArea>();
            if (damageArea == null) {
                return;
            }

            _nextDamageTime.Remove(damageArea);
        }

    }

}
