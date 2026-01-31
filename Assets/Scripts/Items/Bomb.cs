using System.Collections;
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

        public bool CanBePickedUp => !_isHeld && _hasBeenThrown;
        public Vector2Int HoldOffset => _holdOffset;
        public bool FlipWithPlayer => _flipWithPlayer;

        protected override void Awake() {
            base.Awake();

            // Bomb always falls (doesn't stop on ground), but does have friction.
            stopOnGround = false;
            applyFriction = true;
            bounces = true;
        }

        private void Start() {
            StartCoroutine(DelayedExplosion());
        }

        protected override void Update() {
            if (_isHeld || _isStuck) {
                return;
            }

            base.Update();
        }

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

        private IEnumerator DelayedExplosion() {
            yield return new WaitForSeconds(timeToExplode - bombTimerClip.length);

            Visuals.animator.Play("BombArmed");

            audioSource.clip = bombTimerClip;
            audioSource.Play();

            yield return new WaitForSeconds(bombTimerClip.length);

            Explode();
        }

        public void Explode(float delay = 0f) {
            StartCoroutine(DoExplode(delay));
        }

        private IEnumerator DoExplode(float delay = 0f) {
            yield return new WaitForSeconds(delay);
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
