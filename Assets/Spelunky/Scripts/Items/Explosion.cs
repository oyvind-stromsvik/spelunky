using System.Collections.Generic;
using UnityEngine;

namespace Spelunky {

    public class Explosion : MonoBehaviour {
        public float explosionRadius;
        public LayerMask layerMask;

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
