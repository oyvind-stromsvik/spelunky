using UnityEngine;

namespace Spelunky {

    [CreateAssetMenu]
    public class SpriteAnimation : ScriptableObject {
        public Sprite[] frames;
        public int fps;
        public bool looping;
        public bool pingPong;
        // Good for things like explosions etc. so we don't have to leave a blank
        // frame at the end of every sprite sheet we make.
        public bool showBlankFrameAtTheEnd;
    }

}
