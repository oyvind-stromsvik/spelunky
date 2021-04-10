using System;
using System.Collections;
using UnityEngine;

namespace Spelunky {

    public class SpriteAnimator : MonoBehaviour {
        public SpriteAnimation[] animations;

        // Set this to override the speed of the current animation.
        // Useful for having a dynamic fps based on move speed etc.
        public int fps;

        // Set this to control the current frame of the animation. F. ex. setting
        // fps to 0 and this to 0 let's us show only the initial frame of the animation
        // for as long as we wish.
        [HideInInspector] public int currentFrame;

        [HideInInspector] public SpriteAnimation currentAnimation;

        private SpriteRenderer _spriteRenderer;
        private float _timer;

        private bool _playOnceUninterrupted;

        // Store this here because we can have a non-looping ping pong animation
        // and in that case we allow it to play once forwards and once in reverse
        // before we stop it.
        private bool _pingPong;
        // If the current animation is set to ping pong and is now supposed to
        // be played in reverse.
        private bool _reverse;

        private void Awake() {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            currentAnimation = animations[0];
        }

        private void Update() {
            // For enabling playing an uninterrupted animation like a weapon
            // attack even if we're telling the animator to play other animations
            // like walking, running or jumping at the same time.
            if (_playOnceUninterrupted) {
                return;
            }

            // Don't do anything if there is no animation assigned.
            if (currentAnimation == null) {
                return;
            }

            // Don't do anything if the current animation has no frames.
            if (currentAnimation.frames.Length == 0) {
                return;
            }

            if (ReachedEndOfAnimation()) {
                if (currentAnimation.showBlankFrameAtTheEnd) {
                    _spriteRenderer.sprite = null;
                }

                // The current animation is set to ping pong so switch its direction.
                if (_pingPong) {
                    _reverse = !_reverse;
                }
                // The animation is not looping.
                if (!currentAnimation.looping) {
                    // If it's ping ponging let it loop once more before we stop it.
                    if (_pingPong) {
                        _pingPong = false;
                    }
                    else {
                        Stop();
                    }
                }
            }

            // This is a really clever way of handling looping.
            // Taken from GameMaker, and inspired by Daniel Linssen's code.
            currentFrame %= currentAnimation.frames.Length;

            // Set the current sprite.
            _spriteRenderer.sprite = currentAnimation.frames[currentFrame];

            // If the framerate is zero don't calculate the next frame.
            // We can change the current frame from outside this script
            // and animate the sprite this way as well.
            if (fps == 0) {
                return;
            }

            IncreaseFrameCount();
        }

        private void IncreaseFrameCount() {
            _timer += Time.deltaTime;
            if (_timer >= (1f / fps)) {
                _timer = 0;
                if (_reverse) {
                    currentFrame--;
                }
                else {
                    currentFrame++;
                }
            }
        }

        public void Play(string name, bool reset = false, float speed = 1) {
            if (currentAnimation != null && currentAnimation.name == name) {
                return;
            }

            bool found = false;
            foreach (SpriteAnimation animation in animations) {
                if (animation.name == name) {
                    currentAnimation = animation;
                    fps = Mathf.RoundToInt(Mathf.Abs(currentAnimation.fps * speed));
                    _reverse = speed < 0;
                    _pingPong = currentAnimation.pingPong;

                    if (reset) {
                        // Switch over to the new animation immediately. Otherwise
                        // there is a 1 frame delay.
                        currentFrame = 0;
                        _spriteRenderer.sprite = currentAnimation.frames[currentFrame];
                    }

                    found = true;
                    break;
                }
            }

            if (!found) {
                Debug.LogError("Animation " + name + " not found.");
            }
        }

        public void PlayOnceUninterrupted(string playOnceName, int playOnceFps) {
            StartCoroutine(DoPlayOnceUninterrupted(playOnceName, playOnceFps));
        }

        private IEnumerator DoPlayOnceUninterrupted(string playOnceName, int playOnceFps) {
            _playOnceUninterrupted = true;

            SpriteAnimation playOnceAnimation = null;
            int playOnceCurrentFrame = 0;

            foreach (SpriteAnimation animation in animations) {
                if (animation.name == playOnceName) {
                    playOnceAnimation = animation;
                    playOnceCurrentFrame = 0;
                    _spriteRenderer.sprite = playOnceAnimation.frames[playOnceCurrentFrame];
                    break;
                }
            }

            if (playOnceAnimation == null) {
                yield break;
            }

            float t = 0;
            while (playOnceCurrentFrame < playOnceAnimation.frames.Length) {
                _spriteRenderer.sprite = playOnceAnimation.frames[playOnceCurrentFrame];
                t += Time.deltaTime;
                if (t >= 1f / playOnceFps) {
                    t = 0;
                    playOnceCurrentFrame++;
                }

                yield return null;
            }

            _playOnceUninterrupted = false;
        }

        /// <summary>
        /// Returns the length of the animation in seconds.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public float GetAnimationLength(string name) {
            if (currentAnimation != null && currentAnimation.name == name) {
                return currentAnimation.frames.Length * (1f / fps);
            }

            foreach (SpriteAnimation animation in animations) {
                if (animation.name == name) {
                    return animation.frames.Length * (1f / fps);
                }
            }

            throw new NullReferenceException();
        }

        /// <summary>
        /// Returns the length of the animation in seconds.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public float GetAnimationLength(string name, int fpsToCheckWith) {
            if (currentAnimation != null && currentAnimation.name == name) {
                return currentAnimation.frames.Length * (1f / fpsToCheckWith);
            }

            foreach (SpriteAnimation animation in animations) {
                if (animation.name == name) {
                    return animation.frames.Length * (1f / fpsToCheckWith);
                }
            }

            throw new NullReferenceException();
        }

        public bool ReachedEndOfAnimation() {
            if (currentFrame == (currentAnimation.frames.Length - 1) && !currentAnimation.looping) {
                return true;
            }

            if (_reverse && currentFrame == 0) {
                return true;
            }

            return false;
        }

        public void Stop() {
            currentAnimation = null;
        }
    }

}
