using UnityEngine;

namespace Spelunky {

    /// <summary>
    /// TODO: Replace with new input system. To be honest I thought I had done that ages ago.
    /// </summary>
    [RequireComponent(typeof(Player))]
    public class PlayerInput : MonoBehaviour {

        public float joystickDeadzone;

        private Player _player;

        private void Start() {
            _player = GetComponent<Player>();
        }

        private void Update() {
            if (_player.CurrentPlayerState.LockInput()) {
                return;
            }

            Vector2 directionalInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            directionalInput.x = Mathf.Abs(directionalInput.x) < joystickDeadzone ? 0 : directionalInput.x;
            directionalInput.y = Mathf.Abs(directionalInput.y) < joystickDeadzone ? 0 : directionalInput.y;
            _player.CurrentPlayerState.OnDirectionalInput(directionalInput);

            _player.sprinting = Input.GetButton("Sprint Keyboard") || Input.GetAxisRaw("Sprint Controller") != 0;

            if (Input.GetButtonDown("Jump")) {
                _player.CurrentPlayerState.OnJumpInputDown();
            }

            if (Input.GetButtonUp("Jump")) {
                _player.CurrentPlayerState.OnJumpInputUp();
            }

            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.Joystick1Button1)) {
                _player.CurrentPlayerState.OnBombInputDown();
            }

            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Joystick1Button3)) {
                _player.CurrentPlayerState.OnRopeInputDown();
            }

            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Joystick1Button5)) {
                _player.CurrentPlayerState.OnUseInputDown();
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.Joystick1Button2)) {
                _player.CurrentPlayerState.OnAttackInputDown();
            }
        }

    }

}