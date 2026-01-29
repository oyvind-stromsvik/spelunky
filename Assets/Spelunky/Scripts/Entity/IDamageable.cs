namespace Spelunky {

    /// <summary>
    /// Marks a component as able to receive damage.
    /// </summary>
    public interface IDamageable {

        bool TryTakeDamage(int damage);

    }

}
