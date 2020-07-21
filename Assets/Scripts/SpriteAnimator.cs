using System;
using UnityEngine;

public class SpriteAnimator : MonoBehaviour {

    public SpriteAnimation[] animations;

    public bool looping;

    // Set this to control the speed of the animation.
    public int fps;

    // Set this to control the current frame of the animation. F. ex. setting
    // fps to 0 and this to 0 let's us show only the initial frame of the animation
    // for as long as we wish.
    [HideInInspector]
    public int currentFrame;

    // The default sprite is the one we've assigned to the sprite renderer.
    // Store it here so we can put it back later if we need to. We'll lose the
    // one on the sprite renderer when we play an animation.
    [HideInInspector]
    public Sprite defaultSprite;

    [HideInInspector]
    public SpriteAnimation currentAnimation;

    private SpriteRenderer _spriteRenderer;
    private float _timer;
    private int _frameCounter;

    private void Awake() {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        defaultSprite = _spriteRenderer.sprite;
        currentAnimation = animations[0];
    }

    private void Update() {
        // Don't do anything if there is no animation assigned.
        if (currentAnimation == null) {
            return;
        }

        // Don't do anything if the current animation has no frames.
        if (currentAnimation.frames.Length == 0) {
            return;
        }

        if (currentFrame == currentAnimation.frames.Length && !looping) {
            return;
        }

        // This is a really clever way of handling looping.
        // Taken from GameMaker, and inspired by Daniel Linssen's code.
        currentFrame %= currentAnimation.frames.Length;

        // If the framerate is zero don't calculate the next frame, just keep
        // returning the current frame. We can change the current frame
        // from outside this script and animate the sprite this way as well.
        if (fps == 0) {
            _spriteRenderer.sprite = currentAnimation.frames[currentFrame];
            return;
        }

        _spriteRenderer.sprite = currentAnimation.frames[currentFrame];

        _timer += Time.deltaTime;
        if (_timer >= (1f / fps)) {
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
}
