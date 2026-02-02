using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spelunky {

    /// <summary>
    /// Manages timers as an explicit replacement for coroutines.
    /// Provides full control over timer execution within the game loop.
    /// </summary>
    public class TimerManager : MonoBehaviour {

        public static TimerManager Instance { get; private set; }

        [Header("Debug")]
        [Tooltip("Enable to show active timers in the inspector")]
        [SerializeField] private bool showDebugInfo = true;

        [SerializeField] private List<DebugTimerInfo> _debugTimers = new List<DebugTimerInfo>();

        private readonly List<Timer> _activeTimers = new List<Timer>();
        private readonly List<Timer> _pendingAdd = new List<Timer>();
        private readonly List<Timer> _pendingRemove = new List<Timer>();
        private bool _isIterating;

        // Stats
        public int ActiveTimerCount => _activeTimers.Count;

        private void Awake() {
            Instance = this;
        }

        private void LateUpdate() {
            if (showDebugInfo) {
                UpdateDebugInfo();
            }
        }

        private void UpdateDebugInfo() {
            _debugTimers.Clear();
            foreach (Timer timer in _activeTimers) {
                if (timer == null || timer.IsCancelled) continue;
                _debugTimers.Add(new DebugTimerInfo {
                    duration = timer.Duration,
                    elapsed = timer.ElapsedTime,
                    remaining = timer.RemainingTime,
                    progress = timer.Progress,
                    isRepeating = timer.IsRepeating
                });
            }
        }

        /// <summary>
        /// Called by GameManager during the timer phase of the game loop.
        /// </summary>
        public void Tick() {
            // Add pending timers
            if (_pendingAdd.Count > 0) {
                _activeTimers.AddRange(_pendingAdd);
                _pendingAdd.Clear();
            }

            // Update active timers
            _isIterating = true;
            for (int i = 0; i < _activeTimers.Count; i++) {
                Timer timer = _activeTimers[i];
                if (timer.IsCancelled) {
                    continue;
                }

                timer.ElapsedTime += Time.deltaTime;
                if (timer.ElapsedTime >= timer.Duration) {
                    timer.OnComplete?.Invoke();
                    if (!timer.IsRepeating) {
                        _pendingRemove.Add(timer);
                    } else {
                        timer.ElapsedTime -= timer.Duration;
                    }
                }
            }
            _isIterating = false;

            // Remove completed/cancelled timers
            if (_pendingRemove.Count > 0) {
                foreach (Timer timer in _pendingRemove) {
                    _activeTimers.Remove(timer);
                }
                _pendingRemove.Clear();
            }
        }

        /// <summary>
        /// Creates a one-shot timer that fires after the specified duration.
        /// </summary>
        public Timer CreateTimer(float duration, Action onComplete) {
            Timer timer = new Timer {
                Duration = duration,
                OnComplete = onComplete,
                IsRepeating = false
            };

            if (_isIterating) {
                _pendingAdd.Add(timer);
            } else {
                _activeTimers.Add(timer);
            }

            return timer;
        }

        /// <summary>
        /// Creates a repeating timer that fires every interval.
        /// </summary>
        public Timer CreateRepeatingTimer(float interval, Action onTick) {
            Timer timer = new Timer {
                Duration = interval,
                OnComplete = onTick,
                IsRepeating = true
            };

            if (_isIterating) {
                _pendingAdd.Add(timer);
            } else {
                _activeTimers.Add(timer);
            }

            return timer;
        }

        /// <summary>
        /// Cancels and removes a timer.
        /// </summary>
        public void CancelTimer(Timer timer) {
            if (timer == null) return;

            timer.IsCancelled = true;
            if (_isIterating) {
                _pendingRemove.Add(timer);
            } else {
                _activeTimers.Remove(timer);
            }
        }

        /// <summary>
        /// Cancels all active timers.
        /// </summary>
        public void CancelAllTimers() {
            foreach (Timer timer in _activeTimers) {
                timer.IsCancelled = true;
            }
            _activeTimers.Clear();
            _pendingAdd.Clear();
            _pendingRemove.Clear();
        }
    }

    /// <summary>
    /// Represents a timer managed by TimerManager.
    /// </summary>
    public class Timer {
        public float Duration { get; set; }
        public float ElapsedTime { get; set; }
        public Action OnComplete { get; set; }
        public bool IsRepeating { get; set; }
        public bool IsCancelled { get; set; }

        /// <summary>
        /// Gets the remaining time until this timer fires.
        /// </summary>
        public float RemainingTime => Mathf.Max(0, Duration - ElapsedTime);

        /// <summary>
        /// Gets the progress of this timer from 0 to 1.
        /// </summary>
        public float Progress => Duration > 0 ? Mathf.Clamp01(ElapsedTime / Duration) : 1f;

        /// <summary>
        /// Cancels this timer.
        /// </summary>
        public void Cancel() {
            TimerManager.Instance?.CancelTimer(this);
        }
    }

    /// <summary>
    /// Debug info struct for displaying active timers in the inspector.
    /// </summary>
    [Serializable]
    public struct DebugTimerInfo {
        public float duration;
        public float elapsed;
        public float remaining;
        [Range(0f, 1f)] public float progress;
        public bool isRepeating;
    }

}
