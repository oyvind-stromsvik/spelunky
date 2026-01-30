using System.Collections.Generic;
using UnityEngine;

namespace Spelunky {

    public class Explosion : MonoBehaviour {

        public float explosionRadius;
        public LayerMask layerMask;

        [Header("Damage")]
        public int damage = 10;

        [Header("Impulse")]
        public float explosionImpulse = 512f;

        public AudioClip bombExplosionClip;

        // References.
        private AudioSource _audioSource;
        private SpriteAnimator _spriteAnimator;

        private void Awake() {
            _audioSource = GetComponent<AudioSource>();
            _spriteAnimator = GetComponentInChildren<SpriteAnimator>();
        }

        public void Start() {
            _spriteAnimator.Play("Explosion");

            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius, layerMask);
            List<Tile> tilesToRemove = new List<Tile>();
            Vector2 explosionCenter = transform.position;
            float safeRadius = Mathf.Max(0.01f, explosionRadius);

            foreach (Collider2D collider in colliders) {
                if (collider.TryGetComponent(out Tile tile)) {
                    tilesToRemove.Add(tile);
                }

                if (collider.TryGetComponent(out Block block)) {
                    Destroy(block.gameObject);
                }

                // Explode any bombs in our explosions radius immediately causing
                // chain reactions.
                Bomb bomb = collider.GetComponent<Bomb>();
                if (bomb != null) {
                    // Add some randomness to when the overlapping bombs explode so they don't all go in the same frame.
                    // This is boring from a gameplay perspective, but it also causes the sound effect to become way too loud.
                    bomb.Explode(Random.Range(0f, 0.2f));
                }

                IDamageable damageable = collider.GetComponentInParent<IDamageable>();
                if (damageable != null) {
                    damageable.TryTakeDamage(damage);
                }

                IImpulseReceiver impulseReceiver = collider.GetComponentInParent<IImpulseReceiver>();
                if (impulseReceiver != null) {
                    Vector2 toTarget = (Vector2)collider.bounds.center - explosionCenter;
                    float distance = toTarget.magnitude;
                    Vector2 direction = distance > 0.001f ? toTarget / distance : Vector2.up;
                    float falloff = 1f - Mathf.Clamp01(distance / safeRadius);
                    Vector2 impulse = direction * explosionImpulse * falloff;
                    impulseReceiver.ApplyImpulse(impulse);
                }
            }

            LevelGenerator.instance.RemoveTiles(tilesToRemove.ToArray());

            _audioSource.clip = bombExplosionClip;
            _audioSource.pitch = Random.Range(0.95f, 1.05f);
            _audioSource.Play();

            // Give both the animation and the audio clip enough time to play before destroying.
            Destroy(gameObject, 2f);
        }

        private void OnDrawGizmos() {
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }

    }

}
