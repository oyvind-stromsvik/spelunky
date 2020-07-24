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

        private Vector2 _velocity;
        private Vector2 _newPos;
        private Vector2 _oldPos;
        private Vector2 _originalPos;
        bool _hasHit;
        private AudioSource _audioSource;

        private void Awake() {
            _audioSource = GetComponent<AudioSource>();
            ropeMiddle.gameObject.SetActive(false);
        }

        private void Start() {
            SetPositionToCenterOfNearestTile();
            _velocity = Vector2.up * ropeSpeed;
            _newPos = transform.position;
            _oldPos = _newPos;
            _originalPos = _newPos;

            ropeEnd.transform.position += Vector3.down * Mathf.RoundToInt(LevelGenerator.instance.TileWidth / 2f);

            _audioSource.clip = ropeTossClip;
            _audioSource.Play();
        }

        private void Update() {
            if (_hasHit) {
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
                    OnHit();
                }
                else {
                    RaycastHit2D hit = Physics2D.Raycast(_oldPos, direction, distanceThisFrame, layerMask);
                    if (hit.collider != null && hit.transform.CompareTag("OneWayPlatform") == false) {
                        OnHit();
                    }
                }
            }

            if (!_hasHit) {
                _oldPos = transform.position;
                transform.position = _newPos;
            }
        }

        private void OnHit() {
            _hasHit = true;
            SetPositionToCenterOfNearestTile();

            // Ensure ropes that are higher up are drawn in front of ropes that
            // are lower down.
            ropeTop.sortingOrder = (int) transform.position.y;
            ropeMiddle.sortingOrder = (int) transform.position.y;
            ropeEnd.sortingOrder = (int) transform.position.y;

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

            ropeMiddle.size = new Vector2(ropeMiddle.size.x, Mathf.FloorToInt(ropeMiddle.size.y / LevelGenerator.instance.TileHeight) * LevelGenerator.instance.TileHeight);
            ropeMiddle.GetComponent<BoxCollider2D>().size = ropeMiddle.size;
            ropeMiddle.GetComponent<BoxCollider2D>().offset = new Vector2(ropeMiddle.GetComponent<BoxCollider2D>().offset.x, -1 * ropeMiddle.size.y / 2f);
            ropeEnd.gameObject.SetActive(false);
        }

        private void SetPositionToCenterOfNearestTile() {
            int x = Mathf.FloorToInt(Mathf.Abs(transform.position.x) / LevelGenerator.instance.TileWidth) * LevelGenerator.instance.TileWidth + Mathf.RoundToInt(LevelGenerator.instance.TileWidth / 2f);
            int y = Mathf.FloorToInt(Mathf.Abs(transform.position.y) / LevelGenerator.instance.TileHeight) * LevelGenerator.instance.TileHeight + Mathf.RoundToInt(LevelGenerator.instance.TileHeight / 2f);
            transform.position = new Vector3(x, y, 0);
        }
    }
}
