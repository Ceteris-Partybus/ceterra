using Mirror;

public abstract class NetworkedSingleton<T> : NetworkBehaviour where T : NetworkBehaviour {
    private static T instance;
    private static readonly object lockObj = new();
    protected virtual bool ShouldPersistAcrossScenes {
        get {
            return false;
        }
    }

    public static T Instance {
        get {
            lock (lockObj) {
                return instance;
            }
        }
    }

    protected virtual void Awake() {
        lock (lockObj) {
            if (instance == null) {
                instance = this as T;
            }
            else if (instance != this) {
                Destroy(gameObject);
                return;
            }
        }
    }

    // Don't call `DontDestroyOnLoad` in Awake as mentioned in
    // https://mirror-networking.gitbook.io/docs/manual/components/networkbehaviour
    protected virtual void Start() {
        if (instance == this && ShouldPersistAcrossScenes) {
            DontDestroyOnLoad(gameObject);
        }
    }
}
