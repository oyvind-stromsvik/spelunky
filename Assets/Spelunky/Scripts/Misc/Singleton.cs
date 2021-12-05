using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Component {
    private static T instance;

    public static T Instance {
        get { return instance; }
    }

    public virtual void Awake() {
        if (instance == null) {
            instance = this as T;
        }
        else {
            Destroy(gameObject);
        }
    }
}
