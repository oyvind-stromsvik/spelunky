using UnityEngine;

namespace Spelunky {

    public interface IThrowable : IHoldable {

        /// <summary>
        /// Multiplier applied to throw velocity. 1 = normal, less than 1 = heavier/slower, greater than 1 = lighter/faster.
        /// </summary>
        float ThrowVelocityMultiplier { get; }

        void OnThrown(Player player, Vector2 velocity, bool affectedByGravity);

    }

}
