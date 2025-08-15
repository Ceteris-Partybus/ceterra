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
    private Label resourceValueLabel;
    private Label fundsValueLabel;
    private ProgressBar enviromentBar;
    private Label enviromentValueLabel;
    private ProgressBar societyBar;
    private Label societyValueLabel;
    private ProgressBar economyBar;
    private Label economyValueLabel;

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

        this.fundsButton = rootElement.Q<Button>("ui-buttons__funds");
        if (this.fundsButton == null) {
            Debug.LogError("Funds button not found");
        }
        else {
            this.fundsButton.clicked += () => {
                Debug.Log("Funds button clicked");
                var modal = new FundsModal(this.fundsModalTemplate, this.fundsHistoryModalTemplate, this.fundsDepositModalTemplate, this.fundsInvestProposalModalTemplate, this.fundsInvestProposalSubmitModalTemplate, this.investProposalVoteModalTemplate);
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

        this.resourceValueLabel = rootElement.Q<Label>("resources-value");
        if (this.resourceValueLabel == null) {
            Debug.LogError("Resource value label not found");
        }
        this.fundsValueLabel = rootElement.Q<Label>("funds-value");
        if (this.fundsValueLabel == null) {
            Debug.LogError("Funds value label not found");
        }

        var enviromentBarContainer = rootElement.Q<TemplateContainer>("environment-bar");
        if (enviromentBarContainer != null) {
            this.enviromentBar = enviromentBarContainer.Children().OfType<ProgressBar>().FirstOrDefault();
            if (this.enviromentBar == null) {
                Debug.LogError("Environment progress bar not found");
            }
        }
        else {
            Debug.LogError("Environment bar container not found");
        }
        this.enviromentValueLabel = rootElement.Q<Label>("environment-bar-value");
        if (this.enviromentValueLabel == null) {
            Debug.LogError("Environment value label not found");
        }

        var societyBarContainer = rootElement.Q<TemplateContainer>("society-bar");
        if (societyBarContainer != null) {
            this.societyBar = societyBarContainer.Children().OfType<ProgressBar>().FirstOrDefault();
            if (this.societyBar == null) {
                Debug.LogError("Society progress bar not found");
            }
        }
        else {
            Debug.LogError("Society bar container not found");
        }
        this.societyValueLabel = rootElement.Q<Label>("society-bar-value");
        if (this.societyValueLabel == null) {
            Debug.LogError("Society value label not found");

        }

        var economyBarContainer = rootElement.Q<TemplateContainer>("economy-bar");
        if (economyBarContainer != null) {
            this.economyBar = economyBarContainer.Children().OfType<ProgressBar>().FirstOrDefault();
            if (this.economyBar == null) {
                Debug.LogError("Economy progress bar not found");
            }
        }
        else {
            Debug.LogError("Economy bar container not found");
        }
        this.economyValueLabel = rootElement.Q<Label>("economy-bar-value");
        if (this.economyValueLabel == null) {
            Debug.LogError("Economy value label not found");
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

    public void AddPlayerToOverview(NetworkPlayer player) {
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

    public void SetCurrentPlayerName(string value) {
        if (this.playerNameLabel != null) {
            this.playerNameLabel.text = value;
        }
    }

    public void SetResourceValue(int value) {
        if (this.resourceValueLabel != null) {
            this.resourceValueLabel.text = value.ToString();
        }
    }

    public void SetFundsValue(int value) {
        if (this.fundsValueLabel != null) {
            this.fundsValueLabel.text = value.ToString();
        }
    }

    public void SetEnvironmentValue(float value) {
        if (this.enviromentBar != null) {
            this.enviromentBar.value = Mathf.Clamp(value, 0, 100);
            this.enviromentBar.title = $"{value} %";
        }
        if (this.enviromentValueLabel != null) {
            this.enviromentValueLabel.text = $"{value}/100";
        }
    }

    public void SetSocietyValue(float value) {
        if (this.societyBar != null) {
            this.societyBar.value = Mathf.Clamp(value, 0, 100);
            this.societyBar.title = $"{value}/100";
        }
        if (this.societyValueLabel != null) {
            this.societyValueLabel.text = $"{value}/100";
        }
    }

    public void SetEconomyValue(float value) {
        if (this.economyBar != null) {
            this.economyBar.value = Mathf.Clamp(value, 0, 100);
            this.economyBar.title = $"{value} %";
        }
        if (this.economyValueLabel != null) {
            this.economyValueLabel.text = $"{value}/100";
        }
    }
}
