namespace Spelunky {

    /// <summary>
    /// Spider enemy that hangs from ceiling, drops when player is below, then jumps toward player.
    /// Uses HangingState -> JumpingState.
    /// </summary>
    public class Spider : Enemy {

        private void Reset() {
            moveSpeed = 0f;
            damage = 1;
            detectionRange = 128f;
        }

    }

}
