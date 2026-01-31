using UnityEngine;

namespace Spelunky {

    public interface IThrowable : IHoldable {

        void OnThrown(Player player, Vector2 velocity, bool affectedByGravity);

    }

}
