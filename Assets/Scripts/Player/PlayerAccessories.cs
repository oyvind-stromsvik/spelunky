using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spelunky {

    public enum AccessoryType {
        ClimbingGlove,
        SpringBoots,
        PitchersMitt,
        Paste
    }

    [RequireComponent(typeof(Player))]
    public class PlayerAccessories : MonoBehaviour {

        public event Action<Sprite> AccessoryAdded;

        private HashSet<AccessoryType> _accessories = new HashSet<AccessoryType>();

        public bool HasClimbingGlove => _accessories.Contains(AccessoryType.ClimbingGlove);
        public bool HasSpringBoots => _accessories.Contains(AccessoryType.SpringBoots);
        public bool HasPitchersMitt => _accessories.Contains(AccessoryType.PitchersMitt);
        public bool HasPaste => _accessories.Contains(AccessoryType.Paste);

        public float JumpHeightBonus => HasSpringBoots ? 16f : 0f;

        public void AddAccessory(AccessoryType type, Sprite icon = null) {
            Debug.Log($"Trying to add {type} accessory.");
            if (_accessories.Add(type)) {
                Debug.Log($"Added {type} accessory.");
                AccessoryAdded?.Invoke(icon);
            }
        }

        public bool HasAccessory(AccessoryType type) {
            return _accessories.Contains(type);
        }

        public void RemoveAccessory(AccessoryType type) {
            _accessories.Remove(type);
        }

    }

}
