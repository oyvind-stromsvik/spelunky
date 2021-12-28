using UnityEngine;

namespace Spelunky {

    /// <summary>
    /// An entity in our game world.
    ///
    /// The idea at the moment is that an entity is anything that isn't a tile.
    ///
    /// I'm not 100% what purpose this class serves. I want everything to be component based and I feel like by just
    /// making prefabs where these scripts are already added I accomplish more or less what I get here, and stuff like
    /// indestructable blocks don't need health for example and visuals are currently only ever used to flip sprites
    /// which some entities don't need either, but it just saves me a few lines of code and just seems easier to work
    /// with.
    ///
    /// We'll see. Maybe I change my mind again later.
    /// </summary>
    [RequireComponent(typeof(EntityPhysics), typeof(EntityHealth), typeof(EntityVisuals))]
    public class Entity : MonoBehaviour {

        public EntityPhysics Physics { get; private set; }
        public EntityHealth Health { get; private set; }
        public EntityVisuals Visuals { get; private set; }

        public virtual void Awake() {
            Physics = GetComponent<EntityPhysics>();
            Health = GetComponent<EntityHealth>();
            Visuals = GetComponent<EntityVisuals>();
        }

    }

}
