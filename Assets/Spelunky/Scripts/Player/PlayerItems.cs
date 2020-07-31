using UnityEngine;

namespace Spelunky {
    [RequireComponent (typeof (Player))]
    public class PlayerItems : MonoBehaviour {

        public bool hasGlove;

        private Player _player;

        private void Start () {
            _player = GetComponent<Player>();
        }

        public void PickupGlove() {
            hasGlove = true;
        }
    }
}
