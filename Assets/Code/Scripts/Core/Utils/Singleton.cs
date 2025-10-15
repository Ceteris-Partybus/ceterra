using Mirror;
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T> {
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
            if (instance != null && instance != this) {
                Destroy(this.gameObject);
                return;
            }
            instance = this as T;
        }
        if (instance == this && ShouldPersistAcrossScenes) {
            DontDestroyOnLoad(gameObject);
        }
        isInitialized = true;
    }
}
