using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class PlayMenuController : MonoBehaviour {
    private UIDocument document;
    private Button backButton;

    private void OnEnable() {
        document = GetComponent<UIDocument>();
        var root = document.rootVisualElement;

        backButton = root.Q<Button>("BackButton");

        backButton.clicked += OnBackButtonClicked;
    }

    private void OnBackButtonClicked() {
        SceneManager.LoadScene("MainMenu");
    }
}