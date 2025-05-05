using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour {
    private UIDocument document;
    private VisualElement root;
    private Button playButton;

    private void OnEnable() {
        document = GetComponent<UIDocument>();
        root = document.rootVisualElement;

        playButton = root.Q<Button>("PlayButton");

        playButton.clicked += OnPlayButtonClicked;
    }

    private void OnPlayButtonClicked() {
        Debug.Log("Play-Button wurde geklickt!");

    }
}