using UnityEngine;
using UnityEngine.Events;

namespace Spelunky {

    [System.Serializable]
    public class UnityIntEvent : UnityEvent<int> {
    }

    [RequireComponent(typeof(Player))]
    public class PlayerInventory : MonoBehaviour {
        public UnityEvent BombsChangedEvent { get; private set; } = new UnityEvent();
        public UnityEvent RopesChangedEvent { get; private set; } = new UnityEvent();
        public UnityIntEvent GoldAmountChangedEvent { get; private set; } = new UnityIntEvent();

        public int numberOfBombs;
        public int numberOfRopes;
        public int goldAmount;

        public bool hasClimbingGlove;
        public bool hasSpringBoots;
        public bool hasPitchersMitt;

        private Player _player;

        private void Reset() {
            numberOfBombs = 4;
            numberOfRopes = 4;
            goldAmount = 0;
        }

        private void Start() {
            _player = GetComponent<Player>();

            BombsChangedEvent?.Invoke();
            RopesChangedEvent?.Invoke();
            GoldAmountChangedEvent?.Invoke(0);
        }

        public void UseBomb() {
            numberOfBombs--;
            BombsChangedEvent?.Invoke();
        }

        public void UseRope() {
            numberOfRopes--;
            RopesChangedEvent?.Invoke();
        }

        public void PickupBombs(int amount) {
            numberOfBombs += amount;
            BombsChangedEvent?.Invoke();
        }

        public void PickupRopes(int amount) {
            numberOfRopes += amount;
            RopesChangedEvent?.Invoke();
        }

        public void PickupGold(int amount) {
            goldAmount += amount;
            GoldAmountChangedEvent?.Invoke(amount);
        }
    }

}