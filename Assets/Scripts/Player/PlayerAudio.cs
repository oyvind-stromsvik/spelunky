using UnityEngine;

public class PlayerAudio : MonoBehaviour {

    public AudioClip jumpClip;
    public AudioClip landClip;
    public AudioClip grabClip;

    // References.
    private AudioSource _audioSource;

    private void Awake() {
        _audioSource = GetComponent<AudioSource>();
    }

    private Player _player;

    private void Start () {
        _player = GetComponent<Player>();
    }

    public void Play(AudioClip clip) {
        _audioSource.clip = clip;
        _audioSource.Play();
    }

}
