using System.Collections.Generic;
using UnityEngine;

namespace Spelunky {
    public class Explosion : MonoBehaviour {

        public float explosionRadius;
        public LayerMask layerMask;

        public AudioClip bombExplosionClip;

        // References.
        private AudioSource _audioSource;

        private void Awake() {
            _audioSource = GetComponent<AudioSource>();
        }

        public void Start() {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius, layerMask);
            List<Tile> tilesToRemove = new List<Tile>();
            foreach (Collider2D collider in colliders) {
                Tile tile = collider.GetComponent<Tile>();
                if (tile != null) {
                    tilesToRemove.Add(tile);
                }

                // Explode any bombs in our explosions radius immediately causing
                // chain reactions.
                Bomb bomb = collider.GetComponent<Bomb>();
                if (bomb != null) {
                    bomb.Explode();
                }
            }

            LevelGenerator.instance.RemoveTiles(tilesToRemove.ToArray());

            _audioSource.clip = bombExplosionClip;
            _audioSource.Play();

            // Give both the animation and the audio clip enough time to play before destroying.
            Destroy(gameObject, 2f);
        }

        private void OnDrawGizmos() {
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }

    }
}
