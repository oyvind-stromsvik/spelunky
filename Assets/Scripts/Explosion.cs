using System.Collections;
using UnityEngine;

public class Explosion : MonoBehaviour {

    // References.
    private SpriteAnimator _animator;

    private void Awake() {
        _animator = GetComponentInChildren<SpriteAnimator>();
    }

    private void Start() {
        StartCoroutine(DestroyWhenAnimationFinishes());
    }

    private IEnumerator DestroyWhenAnimationFinishes() {
        yield return new WaitForSeconds(_animator.GetAnimationLength("Explosion"));
        Destroy(gameObject);
    }
}
