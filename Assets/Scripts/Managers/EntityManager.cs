using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spelunky {

    /// <summary>
    /// Manages entity updates (Player, Enemies, PhysicsBodies) in the physics phase.
    /// </summary>
    public class EntityManager : MonoBehaviour {

        public static EntityManager Instance { get; private set; }

        [Header("Debug")]
        [Tooltip("Enable to show registered entities in the inspector")]
        [SerializeField] private bool showDebugInfo = true;

        // Debug display lists (populated in editor for inspection)
        [SerializeField] private List<DebugEntityInfo> _debugEntities = new List<DebugEntityInfo>();
        [SerializeField] private List<DebugEntityInfo> _debugEarlyTickables = new List<DebugEntityInfo>();
        [SerializeField] private List<DebugEntityInfo> _debugLateTickables = new List<DebugEntityInfo>();

        private readonly List<ITickable> _entities = new List<ITickable>();
        private readonly List<ITickable> _pendingAdd = new List<ITickable>();
        private readonly List<ITickable> _pendingRemove = new List<ITickable>();
        private bool _isIterating;

        // Separate lists for specialized tick types
        private readonly List<IEarlyTickable> _earlyTickables = new List<IEarlyTickable>();
        private readonly List<IEarlyTickable> _pendingAddEarly = new List<IEarlyTickable>();
        private readonly List<IEarlyTickable> _pendingRemoveEarly = new List<IEarlyTickable>();

        private readonly List<ILateTickable> _lateTickables = new List<ILateTickable>();
        private readonly List<ILateTickable> _pendingAddLate = new List<ILateTickable>();
        private readonly List<ILateTickable> _pendingRemoveLate = new List<ILateTickable>();

        // Stats
        public int EntityCount => _entities.Count;
        public int EarlyTickableCount => _earlyTickables.Count;
        public int LateTickableCount => _lateTickables.Count;

        private void Awake() {
            Instance = this;
        }

        private void LateUpdate() {
            if (showDebugInfo) {
                UpdateDebugInfo();
            }
        }

        private void UpdateDebugInfo() {
            _debugEntities.Clear();
            foreach (ITickable entity in _entities) {
                _debugEntities.Add(DebugEntityInfo.FromTickable(entity));
            }

            _debugEarlyTickables.Clear();
            foreach (IEarlyTickable tickable in _earlyTickables) {
                _debugEarlyTickables.Add(DebugEntityInfo.FromObject(tickable));
            }

            _debugLateTickables.Clear();
            foreach (ILateTickable tickable in _lateTickables) {
                _debugLateTickables.Add(DebugEntityInfo.FromTickable(tickable));
            }
        }

        /// <summary>
        /// Called by GameManager during the input phase.
        /// </summary>
        public void EarlyTick() {
            // Add pending early tickables
            if (_pendingAddEarly.Count > 0) {
                _earlyTickables.AddRange(_pendingAddEarly);
                _pendingAddEarly.Clear();
            }

            // Remove pending early tickables
            if (_pendingRemoveEarly.Count > 0) {
                foreach (IEarlyTickable tickable in _pendingRemoveEarly) {
                    _earlyTickables.Remove(tickable);
                }
                _pendingRemoveEarly.Clear();
            }

            // Update all early tickables
            _isIterating = true;
            for (int i = 0; i < _earlyTickables.Count; i++) {
                _earlyTickables[i].EarlyTick();
            }
            _isIterating = false;
        }

        /// <summary>
        /// Called by GameManager during the physics phase.
        /// </summary>
        public void Tick() {
            // Add pending entities
            if (_pendingAdd.Count > 0) {
                _entities.AddRange(_pendingAdd);
                _pendingAdd.Clear();
            }

            // Remove pending entities
            if (_pendingRemove.Count > 0) {
                foreach (ITickable entity in _pendingRemove) {
                    _entities.Remove(entity);
                }
                _pendingRemove.Clear();
            }

            // Update all active entities
            _isIterating = true;
            for (int i = 0; i < _entities.Count; i++) {
                ITickable entity = _entities[i];
                if (entity != null && entity.IsTickActive) {
                    entity.Tick();
                }
            }
            _isIterating = false;
        }

        /// <summary>
        /// Called by GameManager during the post-physics phase.
        /// </summary>
        public void LateTick() {
            // Add pending late tickables
            if (_pendingAddLate.Count > 0) {
                _lateTickables.AddRange(_pendingAddLate);
                _pendingAddLate.Clear();
            }

            // Remove pending late tickables
            if (_pendingRemoveLate.Count > 0) {
                foreach (ILateTickable tickable in _pendingRemoveLate) {
                    _lateTickables.Remove(tickable);
                }
                _pendingRemoveLate.Clear();
            }

            // Update all late tickables
            _isIterating = true;
            for (int i = 0; i < _lateTickables.Count; i++) {
                ILateTickable tickable = _lateTickables[i];
                if (tickable != null && tickable.IsTickActive) {
                    tickable.LateTick();
                }
            }
            _isIterating = false;
        }

        /// <summary>
        /// Registers an entity to be updated by this manager.
        /// </summary>
        public void Register(ITickable entity) {
            if (entity == null) return;

            if (_isIterating) {
                if (!_pendingAdd.Contains(entity) && !_entities.Contains(entity)) {
                    _pendingAdd.Add(entity);
                }
            } else {
                if (!_entities.Contains(entity)) {
                    _entities.Add(entity);
                }
            }

            // Also register for early tick if applicable
            if (entity is IEarlyTickable earlyTickable) {
                if (_isIterating) {
                    if (!_pendingAddEarly.Contains(earlyTickable) && !_earlyTickables.Contains(earlyTickable)) {
                        _pendingAddEarly.Add(earlyTickable);
                    }
                } else {
                    if (!_earlyTickables.Contains(earlyTickable)) {
                        _earlyTickables.Add(earlyTickable);
                    }
                }
            }

            // Also register for late tick if applicable
            if (entity is ILateTickable lateTickable) {
                if (_isIterating) {
                    if (!_pendingAddLate.Contains(lateTickable) && !_lateTickables.Contains(lateTickable)) {
                        _pendingAddLate.Add(lateTickable);
                    }
                } else {
                    if (!_lateTickables.Contains(lateTickable)) {
                        _lateTickables.Add(lateTickable);
                    }
                }
            }
        }

        /// <summary>
        /// Unregisters an entity from this manager.
        /// </summary>
        public void Unregister(ITickable entity) {
            if (entity == null) return;

            if (_isIterating) {
                _pendingRemove.Add(entity);
                _pendingAdd.Remove(entity);
            } else {
                _entities.Remove(entity);
            }

            // Also unregister from early tick if applicable
            if (entity is IEarlyTickable earlyTickable) {
                if (_isIterating) {
                    _pendingRemoveEarly.Add(earlyTickable);
                    _pendingAddEarly.Remove(earlyTickable);
                } else {
                    _earlyTickables.Remove(earlyTickable);
                }
            }

            // Also unregister from late tick if applicable
            if (entity is ILateTickable lateTickable) {
                if (_isIterating) {
                    _pendingRemoveLate.Add(lateTickable);
                    _pendingAddLate.Remove(lateTickable);
                } else {
                    _lateTickables.Remove(lateTickable);
                }
            }
        }

        /// <summary>
        /// Registers an early tickable entity (for input processing before physics).
        /// Use this for entities that only need EarlyTick and don't implement ITickable.
        /// </summary>
        public void RegisterEarlyTickable(IEarlyTickable earlyTickable) {
            if (earlyTickable == null) return;

            if (_isIterating) {
                if (!_pendingAddEarly.Contains(earlyTickable) && !_earlyTickables.Contains(earlyTickable)) {
                    _pendingAddEarly.Add(earlyTickable);
                }
            } else {
                if (!_earlyTickables.Contains(earlyTickable)) {
                    _earlyTickables.Add(earlyTickable);
                }
            }
        }

        /// <summary>
        /// Unregisters an early tickable entity.
        /// </summary>
        public void UnregisterEarlyTickable(IEarlyTickable earlyTickable) {
            if (earlyTickable == null) return;

            if (_isIterating) {
                _pendingRemoveEarly.Add(earlyTickable);
                _pendingAddEarly.Remove(earlyTickable);
            } else {
                _earlyTickables.Remove(earlyTickable);
            }
        }
    }

}
