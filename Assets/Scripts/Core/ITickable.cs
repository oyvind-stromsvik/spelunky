using System;
using UnityEngine;

namespace Spelunky {

    /// <summary>
    /// Interface for entities that are updated by the centralized game loop.
    /// Entities implement this and register with their appropriate manager.
    /// </summary>
    public interface ITickable {
        /// <summary>
        /// Whether this tickable should be updated. When false, the manager skips this entity.
        /// </summary>
        bool IsTickActive { get; }

        /// <summary>
        /// Called by the manager during the physics phase of the game loop.
        /// </summary>
        void Tick();
    }

    /// <summary>
    /// Extended interface for entities that need post-physics processing.
    /// </summary>
    public interface ILateTickable : ITickable {
        /// <summary>
        /// Called after the main physics phase, for post-physics adjustments.
        /// </summary>
        void LateTick();
    }

    /// <summary>
    /// Interface for input processing that runs before physics.
    /// </summary>
    public interface IEarlyTickable {
        /// <summary>
        /// Called before physics, typically for input gathering.
        /// </summary>
        void EarlyTick();
    }

    /// <summary>
    /// Debug info struct for displaying registered entities in the inspector.
    /// </summary>
    [Serializable]
    public struct DebugEntityInfo {
        public string name;
        public string type;
        public bool isActive;
        public GameObject gameObject;

        public static DebugEntityInfo FromTickable(ITickable tickable) {
            MonoBehaviour mb = tickable as MonoBehaviour;
            return new DebugEntityInfo {
                name = mb != null ? mb.name : tickable?.ToString() ?? "null",
                type = tickable?.GetType().Name ?? "null",
                isActive = tickable?.IsTickActive ?? false,
                gameObject = mb != null ? mb.gameObject : null
            };
        }

        public static DebugEntityInfo FromObject(object obj) {
            MonoBehaviour mb = obj as MonoBehaviour;
            ITickable tickable = obj as ITickable;
            return new DebugEntityInfo {
                name = mb != null ? mb.name : obj?.ToString() ?? "null",
                type = obj?.GetType().Name ?? "null",
                isActive = tickable?.IsTickActive ?? true,
                gameObject = mb != null ? mb.gameObject : null
            };
        }
    }

}
