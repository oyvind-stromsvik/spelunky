using UnityEngine;

namespace Spelunky {

    /// <summary>
    /// Base class for enemy AI states. Attach to the same GameObject as the Enemy component.
    /// States are disabled by default and enabled/disabled by the state machine.
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    public abstract class EnemyState : MonoBehaviour, IState {

        protected Enemy enemy;

        protected virtual void Awake() {
            enemy = GetComponent<Enemy>();
            enabled = false;
        }

        /// <summary>
        /// Check if we can enter this state.
        /// </summary>
        public virtual bool CanEnterState() {
            return true;
        }

        /// <summary>
        /// Called when entering this state. Use for initialization.
        /// </summary>
        public virtual void EnterState() {
        }

        /// <summary>
        /// Called when exiting this state. Use for cleanup.
        /// </summary>
        public virtual void ExitState() {
        }

        /// <summary>
        /// Called every frame while this state is active.
        /// </summary>
        public virtual void UpdateState() {
        }

        /// <summary>
        /// Called when a physics collision occurs.
        /// </summary>
        public virtual void OnCollisionEnter(CollisionInfo collisionInfo) {
        }

        /// <summary>
        /// Called when a trigger is entered.
        /// </summary>
        public virtual void OnTriggerEnter(Collider2D other) {
        }

    }

}
