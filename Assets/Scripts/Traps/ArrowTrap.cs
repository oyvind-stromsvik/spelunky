using UnityEngine;

namespace Spelunky {

    public class ArrowTrap : MonoBehaviour {

        [SerializeField] private ThrowableItem arrowPrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float arrowSpeed = 200f;
        [SerializeField] private int fireDirection = -1;
        [SerializeField] private float rayDistance = 160f;
        [SerializeField] private LayerMask detectMask;
        [SerializeField] private AudioClip fireSound;

        private bool _hasFired;

        private Vector2 RayOrigin => firePoint != null ? firePoint.position : transform.position;
        private Vector2 RayDirection => new Vector2(fireDirection, 0);

        private void Update() {
            if (_hasFired) {
                return;
            }

            RaycastHit2D hit = Physics2D.Raycast(RayOrigin, RayDirection, rayDistance, detectMask);
            if (hit.collider != null) {
                Fire();
            }
        }

        private void Fire() {
            _hasFired = true;

            ThrowableItem arrow = Instantiate(arrowPrefab, RayOrigin, Quaternion.identity);

            Vector2 velocity = new Vector2(arrowSpeed * fireDirection, 0);
            arrow.OnThrown(null, velocity, true);

            if (fireSound != null && AudioManager.Instance != null) {
                AudioManager.Instance.PlaySoundAtPosition(fireSound, transform.position, AudioManager.AudioGroup.SFX);
            }
        }

        private void OnDrawGizmosSelected() {
            Gizmos.color = Color.red;
            Vector3 origin = firePoint != null ? firePoint.position : transform.position;
            Vector3 end = origin + new Vector3(fireDirection * rayDistance, 0, 0);
            Gizmos.DrawLine(origin, end);
        }

    }

}
