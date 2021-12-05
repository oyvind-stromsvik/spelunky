using UnityEngine;

namespace Spelunky {

    public class CameraFollow : MonoBehaviour {
        public float verticalOffset;
        public float verticalSmoothTime;
        public Vector2 focusAreaSize;

        private FocusArea _focusArea;

        private Vector3 _smoothVelocity;

        private float _initialVerticalOffset;
        private Player _target;

        public void Initialize(Player player) {
            _target = player;
            _focusArea = new FocusArea(_target.Physics.Collider.bounds, focusAreaSize);
            _initialVerticalOffset = verticalOffset;
        }

        public void SetVerticalOffset(float offset) {
            verticalOffset = _initialVerticalOffset + offset;
        }

        private void LateUpdate() {
            if (_target == null) {
                return;
            }

            _focusArea.Update(_target.Physics.Collider.bounds);
            Vector3 focusPosition = _focusArea.centre + Vector2.up * verticalOffset;
            focusPosition = Vector3.SmoothDamp(transform.position, focusPosition, ref _smoothVelocity, verticalSmoothTime);
            float x = focusPosition.x;
            float y = focusPosition.y;
            if (x < 112) {
                x = 112;
            }

            if (x > 640 - 112) {
                x = 640 - 112;
            }

            if (y < 52) {
                y = 52;
            }

            if (y > 512 - 52) {
                y = 512 - 52;
            }

            Vector3 position = new Vector3(x, y, -10);
            transform.position = position;
        }

        private void OnDrawGizmosSelected() {
            Gizmos.color = new Color(1, 0, 0, .5f);
            Gizmos.DrawCube(_focusArea.centre, focusAreaSize);
        }

        private struct FocusArea {
            public Vector2 centre;
            private float _left;
            private float _right;
            private float _top;
            private float _bottom;

            public FocusArea(Bounds targetBounds, Vector2 size) {
                _left = targetBounds.center.x - size.x / 2;
                _right = targetBounds.center.x + size.x / 2;
                _bottom = targetBounds.min.y;
                _top = targetBounds.min.y + size.y;
                centre = new Vector2((_left + _right) / 2, (_top + _bottom) / 2);
            }

            public void Update(Bounds targetBounds) {
                float shiftX = 0;
                if (targetBounds.min.x < _left) {
                    shiftX = targetBounds.min.x - _left;
                }
                else if (targetBounds.max.x > _right) {
                    shiftX = targetBounds.max.x - _right;
                }

                _left += shiftX;
                _right += shiftX;

                float shiftY = 0;
                if (targetBounds.min.y < _bottom) {
                    shiftY = targetBounds.min.y - _bottom;
                }
                else if (targetBounds.max.y > _top) {
                    shiftY = targetBounds.max.y - _top;
                }

                _top += shiftY;
                _bottom += shiftY;
                centre = new Vector2((_left + _right) / 2, (_top + _bottom) / 2);
            }
        }
    }

}