namespace Spelunky {

    /// <summary>
    /// Bat enemy that hangs idle until triggered, then flies toward the player.
    /// Uses IdleState -> ChaseState (Flying mode).
    /// </summary>
    public class Bat : Enemy {

        private void Reset() {
            moveSpeed = 24f;
            damage = 1;
        }

    }

}
