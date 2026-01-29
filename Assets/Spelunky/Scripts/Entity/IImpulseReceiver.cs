using UnityEngine;

namespace Spelunky {

    /// <summary>
    /// Marks a component as able to receive velocity impulses (explosions, knockback).
    /// </summary>
    public interface IImpulseReceiver {

        void ApplyImpulse(Vector2 impulse);

    }

}
