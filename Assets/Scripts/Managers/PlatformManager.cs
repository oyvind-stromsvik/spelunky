using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spelunky {

    /// <summary>
    /// Manages MovingPlatform updates in the pre-physics phase.
    /// Platforms must run before entities so riders receive their externalDelta.
    /// </summary>
    public class PlatformManager : MonoBehaviour {

        public static PlatformManager Instance { get; private set; }

        [Header("Debug")]
        [Tooltip("Enable to show registered platforms in the inspector")]
        [SerializeField] private bool showDebugInfo = true;

        [SerializeField] private List<DebugPlatformInfo> _debugPlatforms = new List<DebugPlatformInfo>();

        private readonly List<MovingPlatform> _platforms = new List<MovingPlatform>();
        private readonly List<MovingPlatform> _pendingAdd = new List<MovingPlatform>();
        private readonly List<MovingPlatform> _pendingRemove = new List<MovingPlatform>();
        private bool _isIterating;

        // Stats
        public int PlatformCount => _platforms.Count;

        private void Awake() {
            Instance = this;
        }

        private void LateUpdate() {
            if (showDebugInfo) {
                UpdateDebugInfo();
            }
        }

        private void UpdateDebugInfo() {
            _debugPlatforms.Clear();
            foreach (MovingPlatform platform in _platforms) {
                _debugPlatforms.Add(new DebugPlatformInfo {
                    name = platform != null ? platform.name : "null",
                    isActive = platform != null && platform.IsTickActive,
                    direction = platform != null ? platform.startDirection.ToString() : "N/A",
                    speed = platform != null ? platform.speed : 0f,
                    gameObject = platform != null ? platform.gameObject : null
                });
            }
        }

        /// <summary>
        /// Called by GameManager during the pre-physics phase.
        /// </summary>
        public void Tick() {
            // Add pending platforms
            if (_pendingAdd.Count > 0) {
                _platforms.AddRange(_pendingAdd);
                _pendingAdd.Clear();
            }

            // Remove pending platforms
            if (_pendingRemove.Count > 0) {
                foreach (MovingPlatform platform in _pendingRemove) {
                    _platforms.Remove(platform);
                }
                _pendingRemove.Clear();
            }

            // Update all active platforms
            _isIterating = true;
            for (int i = 0; i < _platforms.Count; i++) {
                MovingPlatform platform = _platforms[i];
                if (platform != null && platform.IsTickActive) {
                    platform.Tick();
                }
            }
            _isIterating = false;
        }

        /// <summary>
        /// Registers a platform to be updated by this manager.
        /// </summary>
        public void Register(MovingPlatform platform) {
            if (platform == null) return;

            if (_isIterating) {
                if (!_pendingAdd.Contains(platform) && !_platforms.Contains(platform)) {
                    _pendingAdd.Add(platform);
                }
            } else {
                if (!_platforms.Contains(platform)) {
                    _platforms.Add(platform);
                }
            }
        }

        /// <summary>
        /// Unregisters a platform from this manager.
        /// </summary>
        public void Unregister(MovingPlatform platform) {
            if (platform == null) return;

            if (_isIterating) {
                _pendingRemove.Add(platform);
                _pendingAdd.Remove(platform);
            } else {
                _platforms.Remove(platform);
            }
        }
    }

    /// <summary>
    /// Debug info struct for displaying registered platforms in the inspector.
    /// </summary>
    [Serializable]
    public struct DebugPlatformInfo {
        public string name;
        public bool isActive;
        public string direction;
        public float speed;
        public GameObject gameObject;
    }

}
