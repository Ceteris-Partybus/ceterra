using Assets.Code.Scripts.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Code.Scripts.UI {
    public class IngameOverlay : MonoBehaviour {
        private ProgressBar playerHealthBar;
        private Label playerHealthValue;
        private Label playerCoinsValue;
        private Label playerNameLabel;
        private VisualElement playerOverview;

        private void Awake() {
            var rootElement = GetComponent<UIDocument>()?.rootVisualElement;
            if (rootElement == null) {
                Debug.LogError("UIDocument or root visual element is null");
                return;
            }

            // Find the client player card
            var clientPlayerCard = rootElement.Q<TemplateContainer>("client-player-card")?
                .Q<VisualElement>("player-card");

            if (clientPlayerCard == null) {
                Debug.LogError("Client player card not found");
                return;
            }

            // Setup player name
            this.playerNameLabel = clientPlayerCard.Q<Label>("player-card__display-name");
            if (this.playerNameLabel == null) {
                Debug.LogError("Player name label not found");
            }

            // Setup health bar
            var healthBarContainer = clientPlayerCard.Q<TemplateContainer>("player-card__health-bar");
            if (healthBarContainer != null) {
                this.playerHealthBar = healthBarContainer.Children().OfType<ProgressBar>().FirstOrDefault();
                if (this.playerHealthBar == null) {
                    Debug.LogError("Health progress bar not found");
                }
            }
            else {
                Debug.LogError("Health bar container not found");
            }

            // Setup health value
            this.playerHealthValue = clientPlayerCard.Q<Label>("player-card__health-value");
            if (this.playerHealthValue == null) {
                Debug.LogError("Health value label not found");
            }

            // Setup coins display
            this.playerCoinsValue = rootElement.Q<Label>("player-card__coins-value");
            if (this.playerCoinsValue == null) {
                Debug.LogError("Coins value label not found");
            }

            // Setup player overview
            this.playerOverview = rootElement.Q<VisualElement>("player-overview");
            if (this.playerOverview == null) {
                Debug.LogError("Player overview not found");
            }
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

        public void UpdateClientHealth(int newHealth) {
            if (this.playerHealthBar != null) {
                this.playerHealthBar.value = Mathf.Clamp(newHealth, 0, 100);
                this.playerHealthBar.title = $"{this.playerHealthBar.value} / 100";
                this.playerHealthBar.ClearClassList();
                this.playerHealthBar.AddToClassList(this.GetHealthBarClassName(newHealth));
                if (this.playerHealthValue != null) {
                    this.playerHealthValue.text = newHealth.ToString() + " / 100";
                }
            }
        }

        public void UpdatePlayerHealth(int newHealth, ulong playerId) {
            Debug.Log($"Updating health for player {playerId} to {newHealth}");
        }

        public void UpdateClientCoins(int newCoins) {
            if (this.playerCoinsValue != null) {
                this.playerCoinsValue.text = newCoins.ToString();
            }
        }

        public void UpdatePlayerCoins(int newCoins, ulong playerId) {
            Debug.Log($"Updating coins for player {playerId} to {newCoins}");
            var playerCard = this.playerOverview.Q<VisualElement>("player-overview-" + playerId.ToString());
            if (playerCard != null) {
                var playerCoinsLabel = playerCard.Q<Label>("player-card__coins-value");
                if (playerCoinsLabel != null) {
                    playerCoinsLabel.text = newCoins.ToString();
                }
            }
        }

        internal void AddPlayerToOverview(NetworkPlayer player) {
            VisualElement playerCardWrapper = new VisualElement();
            playerCardWrapper.name = "player-overview-" + player.NetworkObjectId.ToString();
            VisualTreeAsset playerCardTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Level/UI/Global/PlayerCard.uxml");
            VisualElement playerCardContainer = playerCardTemplate.Instantiate();
            var playerCard = playerCardContainer.Q<VisualElement>("player-card");
            var playerNameLabel = playerCard.Q<Label>("player-card__display-name");

            var healthBarContainer = playerCard.Q<TemplateContainer>("player-card__health-bar");
            if (healthBarContainer != null) {
                var playerHealthBar = healthBarContainer.Children().OfType<ProgressBar>().FirstOrDefault();
                if (playerHealthBar != null) {
                    playerHealthBar.value = Mathf.Clamp(player.health.Value, 0, 100);
                    playerHealthBar.title = $"{playerHealthBar.value} / 100";
                    playerHealthBar.ClearClassList();
                    playerHealthBar.AddToClassList(this.GetHealthBarClassName(player.health.Value));
                }
            }
            var healthBarLabel = playerCard.Q<Label>("player-card__health-value");
            if (healthBarLabel != null) {
                healthBarLabel.text = player.health.Value.ToString() + " / 100";
            }

            var playerCoinsLabel = playerCard.Q<Label>("player-card__coins-value");
            if (playerCoinsLabel != null) {
                playerCoinsLabel.text = player.coins.Value.ToString();
            }

            playerNameLabel.text = "Player " + player.NetworkObjectId;
            playerCardWrapper.Add(playerCardContainer);
            playerOverview.Add(playerCardWrapper);
        }

        internal void UpdateClientName(string value) {
            if (this.playerNameLabel != null) {
                this.playerNameLabel.text = value;
            }
        }
    }
}