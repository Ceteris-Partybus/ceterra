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

    public AudioSource DiceAudioSource;
    public AudioClip DiceStopSound;

    public AudioSource MoneyHealthSource;
    public AudioClip MoneyHealthSound;

    public AudioSource FailSource;
    public AudioClip FailSound;

    public AudioSource SuccessSource;
    public AudioClip SuccessSound;


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

    public void PlayDiceStopSound() {
        PlaySound(DiceAudioSource, DiceStopSound, "DiceStopSound");
    }

    public void PlayMoneyHealthSound() {
        PlaySound(MoneyHealthSource, MoneyHealthSound, "MoneyHealthSound");
    }

    public void PlayFailSound() {
        PlaySound(FailSource, FailSound, "FailSound");
    }

    public void PlaySuccessSound() {
        PlaySound(SuccessSource, SuccessSound, "SuccessSound");
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

