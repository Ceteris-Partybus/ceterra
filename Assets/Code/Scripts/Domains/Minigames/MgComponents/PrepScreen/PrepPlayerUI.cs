using UnityEngine.UIElements;

public class PrepPlayerUI {

    Button readyButton;
    SceneConditionalPlayer player;
    public bool isPlayerReady => player.IsReady;

    public PrepPlayerUI(Button button, SceneConditionalPlayer player) {
        readyButton = button;
        readyButton.clicked += OnButtonClicked;
        readyButton.SetEnabled(true);
        readyButton.clicked += () => player.CmdChangeReadyState();
        this.player = player;
    }

    public void OnButtonClicked() {
        if (readyButton.text == "Nicht bereit") {
            OnReady();
        }
        else {
            OnNotReady();
        }
    }

    public void OnReady() {
        readyButton.text = "Bereit";
        if (readyButton.ClassListContains("not-ready")) {
            readyButton.RemoveFromClassList("not-ready");
        }
    }

    public void OnNotReady() {
        readyButton.text = "Nicht bereit";
        if (!readyButton.ClassListContains("not-ready")) {
            readyButton.AddToClassList("not-ready");
        }
    }
}