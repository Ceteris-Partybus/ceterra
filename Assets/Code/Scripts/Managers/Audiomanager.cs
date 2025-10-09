using UnityEngine;

public class Audiomanager : MonoBehaviour {
    public static Audiomanager Instance;

    [Header("UI Sounds")]
    public AudioSource ClickAudioSource;
    public AudioClip buttonClickSound;
    
    public AudioSource InvestAudioSource;
    public AudioClip InvestClickSound;
    
    public AudioSource FundsAudioSource;
    public AudioClip FundsClickSound;

    public AudioSource RessourceAudioSource;
    public AudioClip RessourceClickSound;

    private void Awake() {
        
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); 
    }

    public void PlayClickSound() {
        PlaySound(ClickAudioSource, buttonClickSound, "ButtonClickSound");
    }

    public void PlayInvestSound() {
        PlaySound(InvestAudioSource, InvestClickSound, "InvestClickSound");
    }

    public void PlayFundsSound() {
        PlaySound(FundsAudioSource, FundsClickSound, "FundsClickSound");
    }

    public void PlayRessourceSound() {
        PlaySound(RessourceAudioSource, RessourceClickSound, "RessourceClickSound");
    }

   
    private void PlaySound(AudioSource source, AudioClip clip, string name) {
        if (source != null && clip != null) {
            source.PlayOneShot(clip);
        }
        else {
            Debug.LogWarning($"AudioSource oder Clip für '{name}' nicht gesetzt!");
        }
    }
}

