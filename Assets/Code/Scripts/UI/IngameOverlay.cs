using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Code.Scripts.UI {
    public class IngameOverlay : MonoBehaviour {
        private ProgressBar playerHealthBar;
        private Label playerCoinsLabel;

        private void Awake() {
            var rootElement = GetComponent<UIDocument>()?.rootVisualElement;
            if (rootElement == null) {
                Debug.LogError("UIDocument or root visual element is null");
                return;
            }

            this.playerHealthBar = rootElement.Q<ProgressBar>("player-card__health-bar");
            this.playerCoinsLabel = rootElement.Q<Label>("player-card__coins-label");
        }

        public void UpdatePlayerHealth(int newHealth) {
            if (this.playerHealthBar != null) {
                this.playerHealthBar.value = Mathf.Clamp(newHealth, 0, 100);
                this.playerHealthBar.title = $"{this.playerHealthBar.value} / 100";
            }
        }

        public void UpdatePlayerCoins(int newCoins) {
            if (this.playerCoinsLabel != null) {
                this.playerCoinsLabel.text = newCoins.ToString();
            }
        }
    }
}