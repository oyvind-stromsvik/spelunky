using UnityEngine;

namespace Spelunky {

    public interface IHoldable {

        bool CanBePickedUp { get; }
        Vector2Int HoldOffset { get; }
        bool FlipWithPlayer { get; }
        Transform transform { get; }

        void OnPickedUp(Player player);
        void OnDropped(Player player);

    }

}
