namespace Spelunky {

    /// <summary>
    /// Caveman enemy that idles until triggered, then charges back and forth.
    /// Uses IdleState -> ChaseState (Ground mode).
    /// </summary>
    public class Caveman : Enemy {

        private void Reset() {
            moveSpeed = 64f;
            damage = 1;
        }

    }

}
