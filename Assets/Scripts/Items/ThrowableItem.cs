using UnityEngine;

namespace Spelunky {

    /// <summary>
    /// Base class for items that can be picked up, held, and thrown.
    /// Extends PhysicsBody for physics behavior and implements IThrowable.
    /// </summary>
    public class ThrowableItem : PhysicsBody, IThrowable, ICrushable {

        [Header("Throwable Settings")]
        [Tooltip("Can this item be picked up immediately, or only after being thrown once?")]
        public bool canPickupBeforeThrown = true;
        [SerializeField] private Vector2Int _holdOffset;
        [SerializeField] private bool _flipWithPlayer;
        [SerializeField] private bool _faceVelocityDirection;
        [SerializeField] private Transform _spriteTransform;

        [Tooltip("Multiplier applied to throw velocity. 1 = normal, <1 = heavier/slower, >1 = lighter/faster.")]
        [Range(0f, 2f)]
        [SerializeField] private float _throwVelocityMultiplier = 1f;

        [Header("Audio")]
        public AudioClip crushClip;

        public float ThrowVelocityMultiplier => _throwVelocityMultiplier;

        protected bool _isHeld;
        protected bool _hasBeenThrown;
        protected bool _noGravity;
        private bool _initialized;

        public virtual bool CanBePickedUp => !_isHeld && (canPickupBeforeThrown || _hasBeenThrown);
        public Vector2Int HoldOffset => _holdOffset;
        public bool FlipWithPlayer => _flipWithPlayer;

        protected virtual void Start() {
            // ThrowableItems should not stop on ground - they need to maintain
            // their throw arc and bounce like bombs do.
            stopOnGround = false;
            _initialized = true;
        }

        public override bool IsTickActive => _initialized && !_isHeld;

        public override void Tick() {
            base.Tick();

            if (_faceVelocityDirection && _spriteTransform != null && Velocity.sqrMagnitude > 0.01f) {
                float angle = Mathf.Atan2(Velocity.y, Velocity.x) * Mathf.Rad2Deg;
                _spriteTransform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }

        protected override void ApplyGravity() {
            if (_noGravity) {
                return;
            }

            base.ApplyGravity();
        }

        public virtual void OnPickedUp(Player player) {
            _isHeld = true;
            enabled = false;
            Physics.Collider.enabled = false;
        }

        public virtual void OnDropped(Player player) {
            _isHeld = false;
            enabled = true;
            Physics.Collider.enabled = true;
            Physics.SetPosition(player.transform.position);
            Velocity = Vector2.zero;
            _noGravity = false;
        }

        public virtual void OnThrown(Player player, Vector2 velocity, bool affectedByGravity) {
            _isHeld = false;
            _hasBeenThrown = true;
            _noGravity = !affectedByGravity;
            enabled = true;
            Physics.Collider.enabled = true;
            Physics.SetPosition(transform.position);
            Velocity = velocity;
        }

        protected override void OnPhysicsCollisionEnter(CollisionInfo collisionInfo) {
            base.OnPhysicsCollisionEnter(collisionInfo);

            // Re-enable gravity after first collision (PitchersMitt effect ends).
            _noGravity = false;
        }

        public bool IsCrushable => true;

        public void Crush() {
            PlayCrushSound();
            Destroy(gameObject);
        }

        private void PlayCrushSound() {
            if (crushClip == null) {
                return;
            }

            if (AudioManager.Instance != null) {
                AudioManager.Instance.PlaySoundAtPosition(crushClip, transform.position, AudioManager.AudioGroup.SFX);
            }
        }

    }

}
