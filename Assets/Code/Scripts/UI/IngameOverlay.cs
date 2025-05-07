using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Code.Scripts.UI {
    public class IngameOverlay : MonoBehaviour {
        private ProgressBar playerHealthBar;
        private Label playerCoinsValue;

        private void Awake() {
            var rootElement = GetComponent<UIDocument>()?.rootVisualElement;
            if (rootElement == null) {
                Debug.LogError("UIDocument or root visual element is null");
                return;
            }

            var clientPlayerCardContainer = rootElement.Q<TemplateContainer>("client-player-card");

            if (clientPlayerCardContainer == null) {
                Debug.LogError("Client player card container not found. Ensure you're using the correct template.");
                return;
            }

            var clientPlayerCard = clientPlayerCardContainer.Q<VisualElement>("player-card");

            var playerHealthBarContainer = clientPlayerCard.Q<TemplateContainer>("player-card__health-bar");

            if (playerHealthBarContainer == null) {
                Debug.LogError("Player health bar container not found. Ensure you're using the correct template.");
                return;
            }

            this.playerHealthBar = playerHealthBarContainer.Children().FirstOrDefault(x => {
                return x is ProgressBar;
            }) as ProgressBar;

            this.playerCoinsValue = rootElement.Q<Label>("player-card__coins-value");
        }

        private string GetHealthBarClassName(int health) {
            if (health > 70) {
                return "health-bar--green";
            }
            else if (health > 35) {
                return "health-bar--yellow";
            }
            else {
                return "health-bar--red";
            }
        }

        public void UpdatePlayerHealth(int newHealth) {
            if (this.playerHealthBar != null) {
                this.playerHealthBar.value = Mathf.Clamp(newHealth, 0, 100);
                this.playerHealthBar.title = $"{this.playerHealthBar.value} / 100";
                this.playerHealthBar.ClearClassList();
                this.playerHealthBar.AddToClassList(this.GetHealthBarClassName(newHealth));
            }
        }

        public void UpdatePlayerCoins(int newCoins) {
            if (this.playerCoinsValue != null) {
                this.playerCoinsValue.text = newCoins.ToString();
            }
        }
    }
}