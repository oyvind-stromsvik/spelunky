namespace Spelunky {

    /// <summary>
    /// Marks an entity as participating in crush interactions (e.g. moving platforms).
    /// </summary>
    public interface ICrushable {
        bool IsCrushable { get; }
        void Crush();
    }

}
