using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class PlayMenuController : MonoBehaviour {
    private UIDocument document;
    private Button backButton;

    private string MAIN_MENU_SCENE = "MainMenu";

    private void OnEnable() {
        document = GetComponent<UIDocument>();
        var root = document.rootVisualElement;

        backButton = root.Q<Button>("BackButton");

        backButton.clicked += OnBackButtonClicked;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            OnBackButtonClicked();
        }
    }

    private void OnBackButtonClicked() {
        SceneManager.LoadScene(MAIN_MENU_SCENE);
    }
}