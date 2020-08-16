using TMPro;
using UnityEngine;

namespace Spelunky {

    [RequireComponent (typeof (Player))]
    public class PlayerUI : MonoBehaviour {

        private Player _player;
        private TMP_Text _lifeAmountText;
        private TMP_Text _bombAmountText;
        private TMP_Text _ropeAmountText;
        private TMP_Text _totalGoldAmountText;
        private TMP_Text _currentGoldAmountText;

        private void Awake () {
            _player = GetComponent<Player>();
            _lifeAmountText = GameObject.Find("LifeAmountText").GetComponent<TMP_Text>();
            _bombAmountText = GameObject.Find("BombAmountText").GetComponent<TMP_Text>();
            _ropeAmountText = GameObject.Find("RopeAmountText").GetComponent<TMP_Text>();
            _totalGoldAmountText = GameObject.Find("TotalGoldAmountText").GetComponent<TMP_Text>();
            _currentGoldAmountText = GameObject.Find("CurrentGoldAmountText").GetComponent<TMP_Text>();

            _player.health.HealthChanged.AddListener(OnHealthChanged);
            _player.inventory.BombsChanged.AddListener(OnBombsChanged);
            _player.inventory.RopesChanged.AddListener(OnRopesChanged);
            _player.inventory.GoldTotalChanged.AddListener(OnGoldTotalChanged);
            _player.inventory.GoldCurrentChanged.AddListener(OnGoldCurrentChanged);
        }

        private void OnHealthChanged() {
            _lifeAmountText.text = _player.health.CurrentHealth.ToString();
        }

        private void OnBombsChanged() {
            _bombAmountText.text = _player.inventory.numberOfBombs.ToString();
        }

        private void OnRopesChanged() {
            _ropeAmountText.text = _player.inventory.numberOfRopes.ToString();
        }

        private void OnGoldTotalChanged() {
            _totalGoldAmountText.text = _player.inventory.goldTotalAmount.ToString();
        }

        private void OnGoldCurrentChanged() {
            _currentGoldAmountText.text = _player.inventory.goldCurrentAmount.ToString();
        }
    }
}
