using System.Collections;
using UnityEngine;

namespace Spelunky {

    public class Rope : MonoBehaviour {
        public AudioClip ropeTossClip;
        public AudioClip ropeHitClip;

        public float maxRopeLength;
        public float ropeSpeed;

        public LayerMask layerMask;

        public SpriteRenderer ropeTop;
        public SpriteRenderer ropeMiddle;
        public SpriteRenderer ropeEnd;

        public Vector3 placePosition;

        private Vector2 _velocity;
        private Vector2 _newPos;
        private Vector2 _oldPos;
        private Vector2 _originalPos;
        private bool _placed;
        private AudioSource _audioSource;

        // Incremented for every new rope. Used for ensuring ropes that are
        // placed later are drawn in front of ropes that already exist.
        private static int _sortingOrder;

        private void Awake() {
            _audioSource = GetComponent<AudioSource>();
            ropeMiddle.gameObject.SetActive(false);
        }

        private void Start() {
            if (placePosition != Vector3.zero) {
                PlaceRope(placePosition);
            }
            else {
                ThrowRope();
            }

            _sortingOrder++;
            ropeTop.sortingOrder = _sortingOrder + 1;
            ropeMiddle.sortingOrder = _sortingOrder;
            ropeEnd.sortingOrder = _sortingOrder + 1;
        }

        private void Update() {
            if (_placed) {
                return;
            }

            // assume we move all the way
            _newPos += _velocity * Time.deltaTime;

            // Check if we hit anything on the way
            Vector2 direction = _newPos - _oldPos;
            float distanceThisFrame = direction.magnitude;
            float totalDistance = Vector2.Distance(transform.position, _originalPos);

            if (distanceThisFrame > 0) {
                if (totalDistance >= maxRopeLength) {
                    PlaceRope(transform.position);
                }
                else {
                    RaycastHit2D hit = Physics2D.Raycast(_oldPos, direction, distanceThisFrame, layerMask);
                    if (hit.collider != null && hit.transform.CompareTag("OneWayPlatform") == false) {
                        PlaceRope(transform.position);
                    }
                }
            }

            if (!_placed) {
                _oldPos = transform.position;
                transform.position = _newPos;
            }
        }

        private void ThrowRope() {
            transform.position = Tile.GetPositionOfCenterOfNearestTile(transform.position);
            _velocity = Vector2.up * ropeSpeed;
            _newPos = transform.position;
            _oldPos = _newPos;
            _originalPos = _newPos;

            ropeEnd.transform.position += Vector3.down * Mathf.RoundToInt(Tile.Width / 2f);

            _audioSource.clip = ropeTossClip;
            _audioSource.Play();
        }

        private void PlaceRope(Vector3 position) {
            _placed = true;
            transform.position = Tile.GetPositionOfCenterOfNearestTile(position);

            StartCoroutine(ExtendRope());

            _audioSource.clip = ropeHitClip;
            _audioSource.Play();
        }

        private IEnumerator ExtendRope() {
            ropeTop.gameObject.SetActive(true);
            ropeMiddle.gameObject.SetActive(true);

            float ropeLength = maxRopeLength;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, maxRopeLength, layerMask);
            if (hit.collider != null && hit.transform.CompareTag("OneWayPlatform") == false) {
                ropeLength = hit.distance;
            }

            while (ropeMiddle.size.y <= ropeLength) {
                ropeMiddle.size += new Vector2(0, ropeSpeed * Time.deltaTime * 0.5f);
                ropeEnd.transform.position = new Vector3(transform.position.x, transform.position.y - ropeMiddle.size.y, 0);
                yield return null;
            }

            ropeMiddle.size = new Vector2(ropeMiddle.size.x, Mathf.FloorToInt(ropeMiddle.size.y / Tile.Height) * Tile.Height);
            ropeMiddle.GetComponent<BoxCollider2D>().size = new Vector2(ropeMiddle.GetComponent<BoxCollider2D>().size.x, ropeMiddle.size.y);
            ropeMiddle.GetComponent<BoxCollider2D>().offset = new Vector2(ropeMiddle.GetComponent<BoxCollider2D>().offset.x, -1 * ropeMiddle.size.y / 2f);
            ropeEnd.gameObject.SetActive(false);
        }
    }

}