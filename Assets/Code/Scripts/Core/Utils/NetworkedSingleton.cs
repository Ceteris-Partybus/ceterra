using Mirror;

public abstract class NetworkedSingleton<T> : NetworkBehaviour where T : NetworkedSingleton<T> {
    private static T instance;
    private static readonly object lockObj = new();
    protected virtual bool ShouldPersistAcrossScenes => false;
    private bool isInitialized;
    public bool IsInitialized => isInitialized;

    public static T Instance {
        get {
            lock (lockObj) {
                return instance;
            }
        }
    }

    protected virtual void Awake() {
        lock (lockObj) {
            instance ??= this as T;
            if (instance != this) {
                Destroy(gameObject);
            }
        }
    }

    // Don't call `DontDestroyOnLoad` in Awake as mentioned in
    // https://mirror-networking.gitbook.io/docs/manual/components/networkbehaviour
    protected virtual void Start() {
        if (instance == this && ShouldPersistAcrossScenes) {
            DontDestroyOnLoad(gameObject);
        }
        isInitialized = true;
    }
}
