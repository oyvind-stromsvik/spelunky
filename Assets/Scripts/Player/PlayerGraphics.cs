using UnityEngine;

public class PlayerGraphics : MonoBehaviour {

    // References.
    public new SpriteRenderer renderer;
    public SpriteAnimator animator;

    private void Awake() {
        renderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<SpriteAnimator>();
    }

    private Player _player;

    private void Start () {
        _player = GetComponent<Player>();
    }

    private void Update() {
        if (_player.directionalInput.x > 0 && !_player.isFacingRight) {
            FlipCharacter();
        }
        else if (_player.directionalInput.x < 0 && _player.isFacingRight) {
            FlipCharacter();
        }
    }

    public void FlipCharacter() {
        renderer.flipX = !renderer.flipX;
        _player.facingDirection *= -1;
        _player.isFacingRight = !_player.isFacingRight;
    }
}
