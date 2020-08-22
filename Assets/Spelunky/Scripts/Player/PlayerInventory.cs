using UnityEngine;
using UnityEngine.Events;

namespace Spelunky {

    [System.Serializable]
    public class UnityIntEvent : UnityEvent<int> { }

    [RequireComponent (typeof (Player))]
    public class PlayerInventory : MonoBehaviour {

        public UnityEvent BombsChanged { get; private set; } = new UnityEvent();
        public UnityEvent RopesChanged { get; private set; } = new UnityEvent();
        public UnityIntEvent GoldAmountChanged { get; private set; } = new UnityIntEvent();

        public int numberOfBombs;
        public int numberOfRopes;
        public int goldAmount;

        public bool hasClimbingGlove;
        public bool hasSpringBoots;
        public bool hasPitchersMitt;

        public Item[] items;

        private Player _player;

        private void Reset() {
            numberOfBombs = 4;
            numberOfRopes = 4;
            goldAmount = 0;
        }

        private void Start () {
            _player = GetComponent<Player>();

            BombsChanged?.Invoke();
            RopesChanged?.Invoke();
            GoldAmountChanged?.Invoke(0);
        }

        public void PickupItem(Item item) {

        }

        public void UseBomb() {
            numberOfBombs--;
            BombsChanged?.Invoke();
        }

        public void UseRope() {
            numberOfRopes--;
            RopesChanged?.Invoke();
        }

        public void PickupBombs(int amount) {
            numberOfBombs += amount;
            BombsChanged?.Invoke();
        }

        public void PickupRopes(int amount) {
            numberOfRopes += amount;
            RopesChanged?.Invoke();
        }

        public void PickupGold(int amount) {
            goldAmount += amount;
            GoldAmountChanged?.Invoke(amount);
        }
    }
}
