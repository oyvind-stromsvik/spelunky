using UnityEngine;

namespace Spelunky {

    public enum DamageDirection {
        Any,
        FromAbove,
        FromBelow,
        FromLeft,
        FromRight
    }

    /// <summary>
    /// Defines a damage volume with simple directional rules (e.g. spikes from above).
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class DamageArea : MonoBehaviour {

        public int damage = 1;
        public DamageDirection direction = DamageDirection.Any;

        [Tooltip("Minimum velocity required to take damage. Axis-based for directional rules.")]
        public float minVelocity = 0f;

        [Tooltip("If above zero, damage is applied repeatedly while overlapping at this interval.")]
        public float damageInterval = 0f;

        public bool CanDamage(EntityPhysics targetPhysics, Collider2D targetCollider) {
            if (targetPhysics == null || targetCollider == null) {
                return false;
            }

            Vector2 velocity = targetPhysics.RequestedVelocity;
            float threshold = Mathf.Max(0f, minVelocity);

            switch (direction) {
                case DamageDirection.FromAbove:
                    return velocity.y < -threshold;
                case DamageDirection.FromBelow:
                    return velocity.y > threshold;
                case DamageDirection.FromLeft:
                    return velocity.x > threshold;
                case DamageDirection.FromRight:
                    return velocity.x < -threshold;
                case DamageDirection.Any:
                default:
                    if (threshold <= 0f) {
                        return true;
                    }
                    return velocity.magnitude >= threshold;
            }
        }

    }

}
