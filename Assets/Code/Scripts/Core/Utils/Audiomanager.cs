using UnityEngine;

public class Audiomanager : MonoBehaviour {
    public static Audiomanager Instance;

    [Header("UI Sounds")]
    [SerializeField] private AudioSource clickAudioSource;
    [SerializeField] private AudioSource investAudioSource;
    [SerializeField] private AudioSource fundsAudioSource;
    [SerializeField] private AudioSource ressourceAudioSource;
    [SerializeField] private AudioSource moneyHealthSource;
    [SerializeField] private AudioSource failSource;
    [SerializeField] private AudioSource successSource;
    [SerializeField] private AudioSource garbageBinOpenSource;
    [SerializeField] private AudioSource garbageBinCloseSource;
    [SerializeField] private AudioSource earthquakeSource;
    [SerializeField] private AudioSource wildfireSource;
    [SerializeField] private AudioSource atomicExplosionSource;
    public AudioSource soundtrack;
    [SerializeField] private AudioClip coinGainClip;
    [SerializeField] private AudioClip healthGainClip;
    [SerializeField] private AudioClip rollingDiceClip;
    [SerializeField] private AudioClip poppingDiceClip;
    [SerializeField] private AudioClip runningPlayerClip;
    [SerializeField] private AudioClip soundtrackClip;



    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlayClickSound() {
        PlaySound(clickAudioSource);
    }

    public void PlayInvestSound() {
        PlaySound(investAudioSource);
    }

    public void PlayFundsSound() {
        PlaySound(fundsAudioSource);
    }

    public void PlayRessourceSound() {
        PlaySound(ressourceAudioSource);
    }

    public void PlayMoneyHealthSound() {
        PlaySound(moneyHealthSource);
    }

    public void PlayFailSound() {
        PlaySound(failSource);
    }

    public void PlaySuccessSound() {
        PlaySound(successSource);
    }

    public void PlayGarbageBinOpenSound() {
        PlaySound(garbageBinOpenSource);
    }

    public void PlayGarbageBinCloseSound() {
        PlaySound(garbageBinCloseSource);
    }

    public void PlayCoinGainSound(BoardPlayer boardPlayer) {
        PlaySound(boardPlayer.SoundSource, coinGainClip, .6f, 1.42f);
    }

    public void PlayHealthGainSound(BoardPlayer boardPlayer) {
        PlaySound(boardPlayer.SoundSource, healthGainClip, .32f, 1.62f);
    }
    public void PlayCoinLossSound(BoardPlayer boardPlayer) {
        //TODO: find proper sound
        PlaySound(boardPlayer.SoundSource, coinGainClip);
    }

    public void PlayHealthLossSound(BoardPlayer boardPlayer) {
        //TODO: find proper sound
        PlaySound(boardPlayer.SoundSource, healthGainClip);
    }

    public void PlayEarthquakeSound() {
        PlaySound(earthquakeSource);
    }

    public void PlayWildfireSound() {
        PlaySound(wildfireSource);
    }

    public void PlayRollingDiceSound(Dice dice) {
        Loop(dice.SoundSource, rollingDiceClip, .2f, 1.2f);
    }

    public void StopRollingDiceSound(Dice dice) {
        if (dice.SoundSource.isPlaying) {
            dice.SoundSource.Stop();
        }
    }

    public void PlayPoppingDiceSound(Dice dice) {
        PlaySound(dice.SoundSource, poppingDiceClip, .1f);
    }

    public void PlayRunningPlayerSound(BoardPlayer boardPlayer) {
        Loop(boardPlayer.SoundSource, runningPlayerClip, 1.32f);
    }

    public void StopPlayerSound(BoardPlayer boardPlayer) {
        if (boardPlayer.SoundSource.isPlaying) {
            boardPlayer.SoundSource.Stop();
        }
    }

    public void PlayAtomicExplosionSound(Vector3 position) {
        PlayAtPosition(atomicExplosionSource, null);
    }

    public void PlayMusic(AudioClip clip) {
        PlaySound(soundtrack, clip, 1f, 1f, true);
    }

    public void PlaySound(AudioSource source) {
        if (source != null && !source.isPlaying) {
            source.loop = false;
            source.PlayOneShot(source.clip);
        }
    }

    public void PlaySound(AudioSource source, AudioClip clip, float volume = 1f, float pitch = 1f, bool loop = false) {
        source.clip = clip;
        source.loop = loop;
        source.volume = volume;
        source.pitch = pitch;
        source.Play();
    }

    public void Loop(AudioSource source, AudioClip clip, float volume = 1f, float pitch = 1f) {
        PlaySound(source, clip, volume, pitch, true);
    }

    public void PlayAtPosition(AudioSource source, BoardPlayer boardPlayer) {
        AudioSource.PlayClipAtPoint(source.clip, boardPlayer.transform.position, source.volume);
    }
}