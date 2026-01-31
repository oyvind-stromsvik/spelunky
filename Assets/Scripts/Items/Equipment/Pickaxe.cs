using System.Collections;
using UnityEngine;

namespace Spelunky {

    [RequireComponent(typeof(PhysicsBody))]
    public class Pickaxe : MonoBehaviour, IEquipment {

        [SerializeField] private int maxUses = 10;
        [SerializeField] private AudioClip hitSound;
        [SerializeField] private AudioClip breakSound;
        [SerializeField] private Animator animator;
        [SerializeField] private float attackDelay = 0.4f;

        private PhysicsBody _physicsBody;
        private int _remainingUses;
        private bool _isHeld;
        private Player _heldByPlayer;

        public bool CanBePickedUp => !_isHeld;
        public Vector2Int HoldOffset => _holdOffset;
        public bool FlipWithPlayer => _flipWithPlayer;

        [SerializeField] private Vector2Int _holdOffset;
        [SerializeField] private bool _flipWithPlayer;
        
        private bool _isAttacking;
        private float _localXScale;

        private void Awake() {
            _physicsBody = GetComponent<PhysicsBody>();
            _remainingUses = maxUses;
            _localXScale = transform.localScale.x;
        }

        private void Update() {
            if (!_isHeld) {
                return;
            }
            
            Vector3 localScale = transform.localScale;
            localScale.x = _localXScale * _heldByPlayer.Visuals.facingDirection;
            transform.localScale = localScale;
        }

        public void OnPickedUp(Player player) {
            _isHeld = true;
            _heldByPlayer = player;
            _physicsBody.enabled = false;
            _physicsBody.Physics.Collider.enabled = false;
        }

        public void OnDropped(Player player) {
            _isHeld = false;
            _heldByPlayer = null;
            _physicsBody.enabled = true;
            _physicsBody.Physics.Collider.enabled = true;
            _physicsBody.Physics.SetPosition(player.transform.position);
        }

        public void Use(Player player) {
            if (_isAttacking) {
                return;
            }
            // Play swing/attack animation.
            animator.Play("Pickaxe_Attack", 0, 0f);
            StartCoroutine(DelayedAttack(player));
        }

        private IEnumerator DelayedAttack(Player player) {
            _isAttacking = true;
            yield return new WaitForSeconds(attackDelay);
            _isAttacking = false;

            // Check if there is a tile directly in front first.
            Vector2 frontPos = (Vector2)player.transform.position + new Vector2(Tile.Width * player.Visuals.facingDirection, 8);
            Tile tile = GetTileAtPosition(frontPos);
            // If not check diagonal down.
            if (tile == null) {
                Vector2 diagonalPos = (Vector2)player.transform.position + new Vector2(Tile.Width * player.Visuals.facingDirection, -8);
                tile = GetTileAtPosition(diagonalPos);
            }

            if (tile != null) {
                LevelGenerator.instance.RemoveTiles(new[] { tile });
                PlaySound(hitSound);
                ConsumeUse();
            }
        }

        private static Tile GetTileAtPosition(Vector2 position) {
            int x = Mathf.FloorToInt(position.x / Tile.Width);
            int y = Mathf.FloorToInt(position.y / Tile.Height);

            Tile[,] tiles = LevelGenerator.instance.Tiles;
            if (x < 0 || x >= tiles.GetLength(0) || y < 0 || y >= tiles.GetLength(1)) {
                return null;
            }

            return tiles[x, y];
        }

        private void ConsumeUse() {
            _remainingUses--;
            if (_remainingUses <= 0) {
                PlaySound(breakSound);
                if (_heldByPlayer != null) {
                    _heldByPlayer.Holding.Drop();
                }
                Destroy(gameObject);
            }
        }

        private void PlaySound(AudioClip clip) {
            if (clip != null && AudioManager.Instance != null) {
                AudioManager.Instance.PlaySoundAtPosition(clip, transform.position, AudioManager.AudioGroup.SFX);
            }
        }

    }

}
