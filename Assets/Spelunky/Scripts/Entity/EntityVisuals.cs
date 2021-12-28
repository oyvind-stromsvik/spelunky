using UnityEngine;

namespace Spelunky {

    public class EntityVisuals : MonoBehaviour {
        public bool isFacingRight = false;
        public float facingDirection = -1;

        // References.
        [HideInInspector] public new SpriteRenderer renderer;
        [HideInInspector] public SpriteAnimator animator;

        private void Awake() {
            // We can't use RequireComponent for these because these could be on a different game object than the one
            // this script is on.
            renderer = GetComponentInChildren<SpriteRenderer>();
            if (renderer == null) {
                Debug.LogError("No SpriteRenderer found on object or in object children.");
            }
            animator = GetComponentInChildren<SpriteAnimator>();
            if (animator == null) {
                Debug.LogError("No SpriteAnimator found on object or in object children.");
            }
        }

        public void FlipCharacter() {
            renderer.flipX = !renderer.flipX;
            facingDirection *= -1;
            isFacingRight = !isFacingRight;
        }
    }

}
