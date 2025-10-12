using UnityEngine;

public class Audiomanager : MonoBehaviour {
    public static Audiomanager Instance;

    [Header("UI Sounds")]
    public AudioSource uiAudioSource;
    public AudioClip buttonClickSound;

    private void Awake() {
        
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); 
    }

    public void PlayClickSound() {
        if (uiAudioSource != null && buttonClickSound != null) {
            uiAudioSource.PlayOneShot(buttonClickSound);
        }
        else {
            Debug.LogWarning("UI AudioSource oder ButtonClickSound nicht gesetzt!");
        }
    }
}
