using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class BoardOverlay : MonoBehaviour {

    [SerializeField] private UIDocument uiDocument;

    [Header("Modals")]
    [Header("Resources")]
    [SerializeField] private VisualTreeAsset resourcesModalTemplate;
    [SerializeField] private VisualTreeAsset resourceHistoryModalTemplate;
    [Header("Funds")]
    [SerializeField] private VisualTreeAsset fundsModalTemplate;
    [SerializeField] private VisualTreeAsset fundsHistoryModalTemplate;
    [SerializeField] private VisualTreeAsset fundsDepositModalTemplate;
    [SerializeField] private VisualTreeAsset fundsInvestProposalModalTemplate;
    [SerializeField] private VisualTreeAsset fundsInvestProposalSubmitModalTemplate;
    [SerializeField] private VisualTreeAsset investProposalVoteModalTemplate;
    [Header("Investions")]
    [SerializeField] private VisualTreeAsset investModalTemplate;

    private VisualElement rootElement;

    private Button resourcesButton;
    private Button fundsButton;
    private Button investButton;

    private ProgressBar playerHealthBar;
    private Label playerHealthValue;
    private Label playerCoinsValue;
    private Label playerNameLabel;
    private VisualElement playerOverview;

    private void OnEnable() {
        this.rootElement = this.uiDocument.rootVisualElement;
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

        // Setup buttons
        this.resourcesButton = rootElement.Q<Button>("ui-buttons__resources");
        if (this.resourcesButton == null) {
            Debug.LogError("Resources button not found");
        }
        else {
            this.resourcesButton.clicked += () => {
                Debug.Log("Resources button clicked");
                var modal = new ResourceModal(this.resourcesModalTemplate, this.resourceHistoryModalTemplate);
                ModalManager.Instance.ShowModal(modal);
            };
        }

        this.fundsButton = rootElement.Q<Button>("ui-buttons__fonds");
        if (this.fundsButton == null) {
            Debug.LogError("Fonds button not found");
        }
        else {
            this.fundsButton.clicked += () => {
                Debug.Log("Fonds button clicked");
                var modal = new FundsModal(this.fundsModalTemplate, this.fundsHistoryModalTemplate, this.fundsDepositModalTemplate, this.fundsInvestProposalModalTemplate, this.fundsInvestProposalSubmitModalTemplate);
                ModalManager.Instance.ShowModal(modal);
            };
        }

        this.investButton = rootElement.Q<Button>("ui-buttons__invest");
        if (this.investButton == null) {
            Debug.LogError("Invest button not found");
        }
        else {
            this.investButton.clicked += () => {
                Debug.Log("Invest button clicked");
                var modal = new InvestModal(this.investModalTemplate, this.fundsInvestProposalSubmitModalTemplate, this.investProposalVoteModalTemplate);
                ModalManager.Instance.ShowModal(modal);
            };
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

    public void SetCurrentPlayerHealth(int newHealth) {
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

    public void SetPlayerHealth(int newHealth, ulong playerId) {
        Debug.Log($"Updating health for player {playerId} to {newHealth}");
        var playerCard = rootElement.Q<TemplateContainer>("client-player-card")?
            .Q<VisualElement>("player-card-" + playerId.ToString());
        var healthBarContainer = playerCard.Q<TemplateContainer>("player-card__health-bar");
        if (healthBarContainer != null) {
            var otherPlayerHealthbar = healthBarContainer.Children().OfType<ProgressBar>().FirstOrDefault();
            if (otherPlayerHealthbar == null) {
                Debug.LogError("Health progress bar not found");
            }
            if (otherPlayerHealthbar != null) {
                otherPlayerHealthbar.value = Mathf.Clamp(newHealth, 0, 100);
                otherPlayerHealthbar.title = $"{otherPlayerHealthbar.value} / 100";
                otherPlayerHealthbar.ClearClassList();
                otherPlayerHealthbar.AddToClassList(this.GetHealthBarClassName(newHealth));
            }
        }
        else {
            Debug.LogError("Health bar container not found");
        }
    }

    public void SetCurrentPlayerCoins(int newCoins) {
        if (this.playerCoinsValue != null) {
            this.playerCoinsValue.text = newCoins.ToString();
        }
    }

    public void SetPlayerCoins(int newCoins, ulong playerId) {
        Debug.Log($"Updating coins for player {playerId} to {newCoins}");
        var playerCard = this.playerOverview.Q<VisualElement>("player-card-" + playerId.ToString());
        if (playerCard == null) {
            Debug.LogError($"Player card for player {playerId} not found");
            return;
        }
        if (playerCard != null) {
            var playerCoinsLabel = playerCard.Q<Label>("player-card__coins-value");
            if (playerCoinsLabel != null) {
                playerCoinsLabel.text = newCoins.ToString();
            }
        }
    }

    internal void AddPlayerToOverview(NetworkPlayer player) {
        VisualElement playerCardWrapper = new VisualElement();
        playerCardWrapper.name = "player-card-" + player.NetworkObjectId.ToString();
        VisualTreeAsset playerCardTemplate = Resources.Load<VisualTreeAsset>("VTA/PlayerCard");
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

    internal void SetCurrentPlayerName(string value) {
        if (this.playerNameLabel != null) {
            this.playerNameLabel.text = value;
        }
    }
}
