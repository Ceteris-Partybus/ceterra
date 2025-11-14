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

    public AudioSource GarbageBinOpenSource;
    public AudioClip GarbageBinOpenSound;

    public AudioSource GarbageBinCloseSource;
    public AudioClip GarbageBinCloseSound;


    private void Awake() {

        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlayClickSound() {
        PlaySound(ClickAudioSource, buttonClickSound);
    }

    public void PlayInvestSound() {
        PlaySound(InvestAudioSource, InvestClickSound);
    }

    public void PlayFundsSound() {
        PlaySound(FundsAudioSource, FundsClickSound);
    }

    public void PlayRessourceSound() {
        PlaySound(RessourceAudioSource, RessourceClickSound);
    }

    public void PlayDiceStopSound() {
        PlaySound(DiceAudioSource, DiceStopSound);
    }

    public void PlayMoneyHealthSound() {
        PlaySound(MoneyHealthSource, MoneyHealthSound);
    }

    public void PlayFailSound() {
        PlaySound(FailSource, FailSound);
    }

    public void PlaySuccessSound() {
        PlaySound(SuccessSource, SuccessSound);
    }

    public void PlayGarbageBinOpenSound() {
        PlaySound(GarbageBinOpenSource, GarbageBinOpenSound);
    }

    public void PlayGarbageBinCloseSound() {
        PlaySound(GarbageBinCloseSource, GarbageBinCloseSound);
    }

    private void PlaySound(AudioSource source, AudioClip clip) {
        if (source != null && clip != null && !source.isPlaying) {
            source.PlayOneShot(clip);
        }
    }
}

