using UnityEngine;

namespace Spelunky {

    [RequireComponent(typeof(EntityPhysics), typeof(EntityHealth), typeof(EntityVisuals))]
    public class Enemy : MonoBehaviour {
        public EntityPhysics EntityPhysics { get; private set; }
        public EntityHealth EntityHealth { get; private set; }
        public EntityVisuals EntityVisuals { get; private set; }

        public virtual void Awake() {
            EntityPhysics = GetComponent<EntityPhysics>();
            EntityHealth = GetComponent<EntityHealth>();
            EntityVisuals = GetComponent<EntityVisuals>();
        }

    }

}
