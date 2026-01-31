namespace Spelunky {

    /// <summary>
    /// Snake enemy that patrols back and forth, attacking players on contact.
    /// Uses the PatrolState for movement behavior.
    /// </summary>
    public class Snake : Enemy {

        private void Reset() {
            moveSpeed = 16f;
            damage = 1;
        }

    }

}
