using UnityEngine;

namespace Spelunky {

    /// <summary>
    /// A throwable bomb that explodes after a delay.
    /// </summary>
    [RequireComponent(typeof(EntityVisuals))]
    public class Bomb : PhysicsBody, ICrushable, IThrowable {

        public Explosion explosion;
        public AudioClip bombTimerClip;
        public float timeToExplode;

        [Header("Audio")]
        public AudioClip crushClip;

        [SerializeField] private Vector2Int _holdOffset;
        [SerializeField] private bool _flipWithPlayer;

        private bool _isHeld;
        private bool _hasBeenThrown;
        private bool _noGravity;
        private bool _hasPaste;
        private bool _isStuck;
        private bool _initialized;

        private Timer _armTimer;
        private Timer _explodeTimer;

        public bool CanBePickedUp => !_isHeld && _hasBeenThrown;
        public Vector2Int HoldOffset => _holdOffset;
        public bool FlipWithPlayer => _flipWithPlayer;
        public float ThrowVelocityMultiplier => 1f;

        protected override void Awake() {
            base.Awake();

            // Bomb always falls (doesn't stop on ground), but does have friction.
            stopOnGround = false;
            applyFriction = true;
            bounces = true;
        }

        private void Start() {
            StartDelayedExplosion();
            _initialized = true;
        }

        public override bool IsTickActive => _initialized && !_isHeld && !_isStuck;

        protected override void ApplyGravity() {
            if (_noGravity) {
                return;
            }

            base.ApplyGravity();
        }

        public void OnPickedUp(Player player) {
            _isHeld = true;
            enabled = false;
            Physics.Collider.enabled = false;
        }

        public void OnDropped(Player player) {
            _isHeld = false;
            _isStuck = false;
            enabled = true;
            Physics.Collider.enabled = true;
            Physics.SetPosition(player.transform.position);
            Velocity = Vector2.zero;
            _noGravity = false;
            _hasPaste = false;
        }

        public void OnThrown(Player player, Vector2 velocity, bool affectedByGravity) {
            _isHeld = false;
            _isStuck = false;
            _hasBeenThrown = true;
            _noGravity = !affectedByGravity;
            _hasPaste = player.Accessories.HasPaste;
            enabled = true;
            Physics.Collider.enabled = true;
            Physics.SetPosition(transform.position);
            Velocity = velocity;
        }

        private void StartDelayedExplosion() {
            float armDelay = timeToExplode - bombTimerClip.length;
            _armTimer = TimerManager.Instance.CreateTimer(armDelay, OnArmTimerComplete);
        }

        private void OnArmTimerComplete() {
            Visuals.animator.Play("BombArmed");

            audioSource.clip = bombTimerClip;
            audioSource.Play();

            _explodeTimer = TimerManager.Instance.CreateTimer(bombTimerClip.length, Explode);
        }

        public void Explode(float delay = 0f) {
            if (delay > 0f) {
                TimerManager.Instance.CreateTimer(delay, DoExplode);
            } else {
                DoExplode();
            }
        }

        private void Explode() {
            DoExplode();
        }

        private void DoExplode() {
            // Cancel any pending timers
            _armTimer?.Cancel();
            _explodeTimer?.Cancel();

            Instantiate(explosion, transform.position + new Vector3(0, 4, 0), Quaternion.identity);
            Destroy(gameObject);
        }

        protected override void OnPhysicsCollisionEnter(CollisionInfo collisionInfo) {
            base.OnPhysicsCollisionEnter(collisionInfo);

            // Re-enable gravity after first collision (PitchersMitt effect ends).
            _noGravity = false;

            // Paste makes the bomb stick on impact instead of bouncing.
            if (_hasPaste) {
                Velocity = Vector2.zero;
                _isStuck = true;
            }
        }

        protected override void PlayBounceSound() {
            // Bomb uses overall velocity magnitude for sound check.
            if (audioSource != null && bounceClip != null && Physics.Velocity.magnitude > bounceSoundThreshold) {
                audioSource.PlayOneShot(bounceClip);
            }
        }

        public bool IsCrushable => true;

        public void Crush() {
            PlayCrushSound();
            Explode();
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
