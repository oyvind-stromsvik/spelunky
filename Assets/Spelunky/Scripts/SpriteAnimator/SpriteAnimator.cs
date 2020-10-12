using System;
using System.Collections;
using UnityEngine;

namespace Spelunky {

    public class SpriteAnimator : MonoBehaviour {
        public SpriteAnimation[] animations;

        public bool looping;

        // Good for things like explosions etc. so we don't have to leave a blank
        // frame at the end of every sprite sheet we make.
        public bool showBlankFrameAfterNonLoopingAnimation;

        // Set this to control the speed of the animation.
        public int fps;

        // Set this to control the current frame of the animation. F. ex. setting
        // fps to 0 and this to 0 let's us show only the initial frame of the animation
        // for as long as we wish.
        [HideInInspector] public int currentFrame;

        [HideInInspector] public SpriteAnimation currentAnimation;

        private SpriteRenderer _spriteRenderer;
        private float _timer;

        private bool _playOnceUninterrupted;

        private void Awake() {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            currentAnimation = animations[0];
        }

        private void Update() {
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

            if (currentFrame == currentAnimation.frames.Length && !looping) {
                if (showBlankFrameAfterNonLoopingAnimation) {
                    _spriteRenderer.sprite = null;
                }

                return;
            }

            // This is a really clever way of handling looping.
            // Taken from GameMaker, and inspired by Daniel Linssen's code.
            currentFrame %= currentAnimation.frames.Length;
            _spriteRenderer.sprite = currentAnimation.frames[currentFrame];

            // If the framerate is zero don't calculate the next frame.
            // We can change the current frame from outside this script
            // and animate the sprite this way as well.
            if (fps == 0) {
                return;
            }

            _timer += Time.deltaTime;
            if (_timer >= 1f / fps) {
                _timer = 0;
                currentFrame++;
            }
        }

        public void Play(string name, bool reset = false) {
            if (currentAnimation != null && currentAnimation.name == name) {
                return;
            }

            bool found = false;
            foreach (SpriteAnimation animation in animations) {
                if (animation.name == name) {
                    currentAnimation = animation;

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
    }

}
