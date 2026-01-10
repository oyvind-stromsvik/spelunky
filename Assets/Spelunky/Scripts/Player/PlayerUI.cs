using System;
using UnityEngine;
using UnityEngine.UI;

namespace Spelunky {

    [RequireComponent(typeof(Player))]
    public class PlayerUI : MonoBehaviour {
        private Player _player;
        private Text _lifeAmountText;
        private Text _bombAmountText;
        private Text _ropeAmountText;
        private Text _totalGoldAmountText;
        private Text _currentGoldAmountText;

        private int _currentGoldAmount;
        private int _totalGoldAmount;

        public float timeBeforeAddingCurrentGoldToTotal;
        private float _goldAddTimer;
        public int goldToAddPerInterval;
        public float goldIntervalTime;
        private float _intervalTimer;

        private GameObject _canvasObject;

        private void Awake() {
            _player = GetComponent<Player>();
            _lifeAmountText = GameObject.Find("LifeAmountText").GetComponent<Text>();
            _bombAmountText = GameObject.Find("BombAmountText").GetComponent<Text>();
            _ropeAmountText = GameObject.Find("RopeAmountText").GetComponent<Text>();
            _totalGoldAmountText = GameObject.Find("TotalGoldAmountText").GetComponent<Text>();
            _currentGoldAmountText = GameObject.Find("CurrentGoldAmountText").GetComponent<Text>();

            _player.Health.HealthChangedEvent.AddListener(OnHealthChanged);
            _player.Inventory.BombsChangedEvent.AddListener(OnBombsChanged);
            _player.Inventory.RopesChangedEvent.AddListener(OnRopesChanged);
            _player.Inventory.GoldAmountChangedEvent.AddListener(OnGoldChanged);

            // The hackiest of hacks to ensure the black background on the HUD actually cover all the elements.
            // Otherwise it doesn't until you pickup the first piece of gold.
            _canvasObject = GameObject.Find("PlayerUICanvas");
            _canvasObject.SetActive(false);
            _canvasObject.SetActive(true);
        }

        private void Update() {
            if (_currentGoldAmount <= 0) {
                _goldAddTimer = 0;
                _currentGoldAmountText.gameObject.SetActive(false);
                return;
            }

            _currentGoldAmountText.gameObject.SetActive(true);

            _goldAddTimer += Time.deltaTime;
            if (_goldAddTimer < timeBeforeAddingCurrentGoldToTotal) {
                return;
            }

            _intervalTimer += Time.deltaTime;
            if (_intervalTimer < goldIntervalTime) {
                return;
            }

            UpdateUIGoldAmount();
            _intervalTimer = 0;
        }

        private void OnHealthChanged() {
            _lifeAmountText.text = _player.Health.CurrentHealth.ToString();
        }

        private void OnBombsChanged() {
            _bombAmountText.text = _player.Inventory.numberOfBombs.ToString();
        }

        private void OnRopesChanged() {
            _ropeAmountText.text = _player.Inventory.numberOfRopes.ToString();
        }

        private void OnGoldChanged(int amount) {
            _goldAddTimer = 0;
            _intervalTimer = 0;
            _currentGoldAmount += amount;
            _totalGoldAmount = _player.Inventory.goldAmount - _currentGoldAmount;
            _currentGoldAmountText.text = " +" + _currentGoldAmount;
            _totalGoldAmountText.text = _totalGoldAmount.ToString();
        }

        private void UpdateUIGoldAmount() {
            int goldToAdd = goldToAddPerInterval > _currentGoldAmount ? _currentGoldAmount : goldToAddPerInterval;
            _currentGoldAmount -= goldToAdd;
            _totalGoldAmount += goldToAdd;
            _currentGoldAmountText.text = " +" + _currentGoldAmount;
            _totalGoldAmountText.text = _totalGoldAmount.ToString();
        }
    }

}
