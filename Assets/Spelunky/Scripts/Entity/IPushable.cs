namespace Spelunky {

    /// <summary>
    /// Marks an entity as pushable by other entities.
    /// </summary>
    public interface IPushable {
        bool TryPush(UnityEngine.Vector2Int step);
    }

}
