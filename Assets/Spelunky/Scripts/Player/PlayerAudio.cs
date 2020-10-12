using UnityEngine;

namespace Spelunky {

    public class PlayerAudio : MonoBehaviour {
        public AudioClip jumpClip;
        public AudioClip landClip;
        public AudioClip grabClip;
        public AudioClip whipClip;

        private AudioSource _audioSource;

        private void Awake() {
            _audioSource = GetComponent<AudioSource>();
        }

        public void Play(AudioClip clip, float volume = 1f) {
            _audioSource.PlayOneShot(clip, volume);
        }
    }

}