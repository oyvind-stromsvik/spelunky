using UnityEngine;

namespace Spelunky {
    [RequireComponent (typeof (Player))]
    public class PlayerInput : MonoBehaviour {

        public float joystickDeadzone;

        private Player _player;

        private void Start () {
            _player = GetComponent<Player>();
        }

        private void Update () {
            if (_player.stateMachine.CurrentState.LockInput()) {
                return;
            }

            Vector2 directionalInput = new Vector2 (Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            directionalInput.x = Mathf.Abs(directionalInput.x) < joystickDeadzone ? 0 : directionalInput.x;
            directionalInput.y = Mathf.Abs(directionalInput.y) < joystickDeadzone ? 0 : directionalInput.y;
            _player.stateMachine.CurrentState.OnDirectionalInput(directionalInput);

            _player.sprinting = Input.GetButton("Sprint Keyboard") || Input.GetAxisRaw("Sprint Controller") != 0;

            if (Input.GetButtonDown("Jump")) {
                _player.stateMachine.CurrentState.OnJumpInputDown();
            }
            if (Input.GetButtonUp("Jump")) {
                _player.stateMachine.CurrentState.OnJumpInputUp();
            }

            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.Joystick1Button1)) {
                _player.stateMachine.CurrentState.OnBombInputDown();
            }
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Joystick1Button3)) {
                _player.stateMachine.CurrentState.OnRopeInputDown();
            }

            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Joystick1Button5)) {
                _player.stateMachine.CurrentState.OnUseInputDown();
            }
        }
    }
}
