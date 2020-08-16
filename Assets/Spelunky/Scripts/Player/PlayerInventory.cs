using UnityEngine;
using UnityEngine.Events;

namespace Spelunky {
    [RequireComponent (typeof (Player))]
    public class PlayerInventory : MonoBehaviour {

        public UnityEvent BombsChanged { get; private set; } = new UnityEvent();
        public UnityEvent RopesChanged { get; private set; } = new UnityEvent();
        public UnityEvent GoldTotalChanged { get; private set; } = new UnityEvent();
        public UnityEvent GoldCurrentChanged { get; private set; } = new UnityEvent();

        public int numberOfBombs;
        public int numberOfRopes;
        public int goldTotalAmount;
        public int goldCurrentAmount;

        public bool hasClimbingGlove;
        public bool hasSpringBoots;
        public bool hasPitchersMitt;

        public Item[] items;

        private Player _player;

        private void Reset() {
            numberOfBombs = 4;
            numberOfRopes = 4;
            goldTotalAmount = 0;
            goldCurrentAmount = 0;
        }

        private void Start () {
            _player = GetComponent<Player>();

            BombsChanged?.Invoke();
            RopesChanged?.Invoke();
            GoldCurrentChanged?.Invoke();
            GoldTotalChanged?.Invoke();
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
            goldCurrentAmount += amount;
            goldTotalAmount = goldCurrentAmount;
            GoldCurrentChanged?.Invoke();
            GoldTotalChanged?.Invoke();
        }
    }
}
