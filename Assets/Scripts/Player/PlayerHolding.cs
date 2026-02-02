using UnityEngine;

namespace Spelunky {

    [RequireComponent(typeof(Player))]
    public class PlayerHolding : MonoBehaviour {
        
        public Transform holdPosition;
        public LayerMask pickupLayerMask;
        public float pickupRadius = 12f;

        private Player _player;
        private SpriteRenderer _heldItemRenderer;
        private int _originalSortingOrder;
        private float _holdPositionXOffset;

        public IHoldable HeldItem { get; private set; }

        public bool IsHoldingItem => HeldItem != null;
        public bool IsHoldingThrowable => HeldItem is IThrowable;
        public bool IsHoldingEquipment => HeldItem is IEquipment;
        
        private bool _originalFlipX;

        private void Awake() {
            _player = GetComponent<Player>();
            _holdPositionXOffset = holdPosition.localPosition.x;
        }

        private void LateUpdate() {
            Vector3 pos = holdPosition.localPosition;
            pos.x = _holdPositionXOffset * _player.Visuals.facingDirection;
            holdPosition.localPosition = pos;

            if (HeldItem != null) {
                int facing = _player.Visuals.facingDirection;
                HeldItem.transform.localPosition = new Vector3(HeldItem.HoldOffset.x * facing, HeldItem.HoldOffset.y, 0);

                if (HeldItem.FlipWithPlayer && _heldItemRenderer != null) {
                    bool invert = _player.Visuals.facingDirection < 0;
                    _heldItemRenderer.flipX = _originalFlipX ^ invert;
                }
            }
        }

        public bool TryPickupNearby() {
            Collider2D[] results = Physics2D.OverlapCircleAll(
                transform.position, pickupRadius, pickupLayerMask);

            foreach (Collider2D collider in results) {
                IHoldable holdable = collider.GetComponent<IHoldable>();
                if (holdable != null && holdable.CanBePickedUp) {
                    return TryPickUp(holdable);
                }
            }

            return false;
        }

        public bool TryPickUp(IHoldable holdable) {
            Debug.Log($"Attempting to pick up {holdable}.");
            
            if (HeldItem != null || !holdable.CanBePickedUp) {
                return false;
            }

            HeldItem = holdable;
            holdable.OnPickedUp(_player);

            holdable.transform.SetParent(holdPosition);
            holdable.transform.localPosition = Vector3.zero;

            _heldItemRenderer = holdable.transform.GetComponentInChildren<SpriteRenderer>();
            if (_heldItemRenderer != null) {
                _originalFlipX = _heldItemRenderer.flipX;
                _originalSortingOrder = _heldItemRenderer.sortingOrder;
                _heldItemRenderer.sortingOrder = _player.Visuals.renderer.sortingOrder + 1;
                Debug.Log($"Sorting order of held item: {_heldItemRenderer.sortingOrder}");
            }

            return true;
        }

        public void Drop() {
            if (HeldItem == null) {
                return;
            }

            HeldItem.transform.SetParent(null);
            HeldItem.OnDropped(_player);
            RestoreRendererState();
            HeldItem = null;
        }

        private void RestoreRendererState() {
            if (_heldItemRenderer != null) {
                _heldItemRenderer.sortingOrder = _originalSortingOrder;
                _heldItemRenderer = null;
            }
        }

        public void ThrowHeldItem(Vector2 velocity) {
            if (HeldItem is not IThrowable throwable) {
                return;
            }

            _player.Visuals.animator.PlayOnceUninterrupted(_player.throwAnimation);

            throwable.transform.SetParent(null);
            bool affectedByGravity = !_player.Accessories.HasPitchersMitt;
            throwable.OnThrown(_player, velocity, affectedByGravity);
            RestoreRendererState();
            HeldItem = null;
        }

        public void UseEquipment() {
            if (HeldItem is IEquipment equipment) {
                equipment.Use(_player);
            }
        }

    }

}
