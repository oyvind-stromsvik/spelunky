using UnityEngine;

public class HangingState : State {

    public Collider2D colliderToHangFrom;

    public override void Enter() {
        base.Enter();

        Vector2 hangPosition = new Vector2(colliderToHangFrom.transform.position.x - 4, colliderToHangFrom.transform.position.y + 4);
        if (player.isFacingRight) {
            if (colliderToHangFrom.transform.position.x < player.transform.position.x) {
                player.graphics.FlipCharacter();
                hangPosition.x += 24;
            }
        }
        else {
            if (colliderToHangFrom.transform.position.x > player.transform.position.x) {
                player.graphics.FlipCharacter();
            }
            else {
                hangPosition.x += 24;
            }
        }
        transform.position = new Vector2(hangPosition.x, hangPosition.y);

        player.graphics.animator.Play("Hang", true);
    }

    private void Update() {
        // The tile we're hanging from was destroyed.
        if (colliderToHangFrom == null) {
            player.stateMachine.AttemptToChangeState(player.inAirState);
            return;
        }

        if (player.directionalInput.y != 0) {
            player._lookTimer += Time.deltaTime;
            if (player.directionalInput.y > 0) {
                player.graphics.animator.Play("HangLookUp");
            }
            if (player._lookTimer > player._timeBeforeLook) {
                float offset = Mathf.Lerp(0, 64f * Mathf.Sign(player.directionalInput.y), Time.deltaTime * 128);
                player.cam.SetVerticalOffset(offset);
            }
        }
        else {
            player._lookTimer = 0;
            player.cam.SetVerticalOffset(0);
            player.graphics.animator.Play("Hang");
        }
    }

    public override void OnDirectionalInput(Vector2 input) {
        input.x = 0;
        base.OnDirectionalInput(input);
    }

    public override void ChangePlayerVelocity(ref Vector2 velocity) {
        velocity = Vector2.zero;
    }
}
