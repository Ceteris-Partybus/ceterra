using UnityEngine;
using UnityEngine.UIElements;

public class PlayerOverviewListener : MonoBehaviour {

    [SerializeField] private Texture2D collapsedIcon;
    [SerializeField] private Texture2D expandedIcon;

    void OnEnable() {
        var root = this.GetComponent<UIDocument>().rootVisualElement;
        Button playerOverviewToggleButton = root.Q<Button>("player-overview-toggle-button");
        VisualElement playerOverview = root.Q<VisualElement>("player-overview");

        playerOverviewToggleButton.clicked += () => {
            CUI.Toggle(playerOverview);
            playerOverviewToggleButton.iconImage = CUI.IsHidden(playerOverview) ? this.collapsedIcon : this.expandedIcon;
        };
    }
}
