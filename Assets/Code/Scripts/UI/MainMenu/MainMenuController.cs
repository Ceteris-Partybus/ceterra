using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.Audio;

public class MainMenuController : MonoBehaviour {
    private VisualElement root;
    private Button playButton;
    private Button settingsButton;
    private Button exitButton;

    [SerializeField]
    private UIDocument uIDocument;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioSource uiAudioSource;
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip Soundtrack;


    private void OnEnable() {
        root = uIDocument.rootVisualElement;

        InitializeUIElements();
        StartCoroutine(ApplyInitialAudioValuesNextFrame());
        Audiomanager.Instance.PlayMusic(Soundtrack);
    }

    private void InitializeUIElements() {
        playButton = root.Q<Button>("PlayButton");
        playButton.clicked += () => {
            Audiomanager.Instance?.PlayClickSound();
            SceneManager.LoadScene("PlayMenu");
        };

        settingsButton = root.Q<Button>("SettingsButton");
        settingsButton.clicked += () => {
            Audiomanager.Instance?.PlayClickSound();
            SettingsController.Instance?.OpenSettingsPanel();
        };

        exitButton = root.Q<Button>("ExitButton");
        exitButton.clicked += () => {
            Audiomanager.Instance?.PlayClickSound();
            Application.Quit();
        };
    }
    private IEnumerator ApplyInitialAudioValuesNextFrame() {
        yield return null;

        float sound = PlayerPrefs.GetFloat("SoundVolume", 100f);
        float music = PlayerPrefs.GetFloat("MusicVolume", 100f);

        SetMixerVolume(sound, "SoundVol");
        SetMixerVolume(music, "MusicVol");
    }

    private void SetMixerVolume(float value, string param) {
        if (audioMixer == null) { return; }

        float normalized = Mathf.Clamp01(value / 100f);
        float dB = (normalized <= .0001f) ? -80f : Mathf.Log10(normalized) * 20f;
        audioMixer.SetFloat(param, dB);
    }
}