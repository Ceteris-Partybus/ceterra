using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class BoardOverlay : NetworkedSingleton<BoardOverlay> {

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
    [Header("Player")]
    [SerializeField] private VisualTreeAsset playerCardTemplate;

    private Dictionary<int, VisualElement> playerElements = new Dictionary<int, VisualElement>();

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
                var modal = new InvestModal(this.investModalTemplate, this.fundsInvestProposalSubmitModalTemplate, this.investProposalVoteModalTemplate, this.fundsDepositModalTemplate);
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

    protected override void Start() {
        base.Start();

        UpdateFundsValue(BoardContext.Instance.FundsStat);
        UpdateResourceValue(BoardContext.Instance.ResourceStat);
        UpdateEconomyValue(BoardContext.Instance.EconomyStat);
        UpdateSocietyValue(BoardContext.Instance.SocietyStat);
        UpdateEnvironmentValue(BoardContext.Instance.EnvironmentStat);
    }

    public bool IsPlayerAdded(int playerId) {
        return playerElements.ContainsKey(playerId);
    }

    public void AddPlayer(BoardPlayer player) {
        var playerElement = playerCardTemplate.CloneTree();
        var card = playerElement.Q<VisualElement>("player-card");
        var playerNameLabel = card.Q<Label>("player-card__display-name");
        var healthBarContainer = card.Q<TemplateContainer>("player-card__health-bar");

        if (healthBarContainer != null) {
            var playerHealthBar = healthBarContainer.Children().OfType<ProgressBar>().FirstOrDefault();
            if (playerHealthBar != null) {
                playerHealthBar.value = BoardPlayer.MAX_HEALTH;
                playerHealthBar.title = $"{BoardPlayer.MAX_HEALTH} / {BoardPlayer.MAX_HEALTH}";
                playerHealthBar.ClearClassList();
                playerHealthBar.AddToClassList(this.GetHealthBarClassName(BoardPlayer.MAX_HEALTH));
            }
        }
        else {
            Debug.LogError("Health bar container not found in player card template");
        }

        var healthBarLabel = card.Q<Label>("player-card__health-value");
        if (healthBarLabel != null) {
            healthBarLabel.text = $"{player.Health} / 100";
        }

        var coinsLabel = card.Q<Label>("player-card__coins-value");
        if (coinsLabel != null) {
            coinsLabel.text = player.Coins.ToString();
        }

        playerNameLabel.text = player.PlayerName;
        playerOverview.Add(playerElement);
        playerElements[player.PlayerId] = playerElement;
    }

    public void RemovePlayer(BoardPlayer player) {
        if (playerElements.TryGetValue(player.PlayerId, out var playerElement)) {
            playerOverview.Remove(playerElement);
            playerElements.Remove(player.PlayerId);
        }
        else {
            Debug.LogWarning($"Player with ID {player.PlayerId} not found in player elements");
        }
    }

    private string GetHealthBarClassName(uint health) {
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

    public void UpdateLocalPlayerHealth(uint newHealth) {
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

    public void UpdateRemotePlayerHealth(uint newHealth, int playerId) {
        if (playerElements.TryGetValue(playerId, out var playerCard)) {
            var healthBarContainer = playerCard.Q<TemplateContainer>("player-card__health-bar");
            if (healthBarContainer != null) {
                var otherPlayerHealthbar = healthBarContainer.Children().OfType<ProgressBar>().FirstOrDefault();
                if (otherPlayerHealthbar == null) {
                    Debug.LogError("Health progress bar not found");
                }
                if (otherPlayerHealthbar != null) {
                    otherPlayerHealthbar.value = newHealth;
                    otherPlayerHealthbar.title = $"{newHealth} / 100";
                    otherPlayerHealthbar.ClearClassList();
                    otherPlayerHealthbar.AddToClassList(this.GetHealthBarClassName(newHealth));
                    var healthTextLabel = playerCard.Q<Label>("player-card__health-value");
                    if (healthTextLabel != null) {
                        healthTextLabel.text = $"{newHealth} / 100";
                    }
                }
            }
        }
    }

    public void UpdateLocalPlayerCoins(uint newCoins) {
        if (this.playerCoinsValue != null) {
            this.playerCoinsValue.text = newCoins.ToString();
        }

        foreach (var modal in ModalManager.Instance.ActiveModals) {
            if (modal.ModalContent.Q<Label>("coins-current-value") is Label coinsValue) {
                coinsValue.text = newCoins.ToString();
            }
        }
    }

    public void UpdateRemotePlayerCoins(uint newCoins, int playerId) {
        if (playerElements.TryGetValue(playerId, out var playerCard)) {
            if (playerCard == null) {
                Debug.LogError($"Player card for player {playerId} not found");
                return;
            }
            var playerCoinsLabel = playerCard.Q<Label>("player-card__coins-value");
            if (playerCoinsLabel != null) {
                playerCoinsLabel.text = newCoins.ToString();
            }
        }
    }

    public void UpdateResourceValue(uint value) {
        if (this.resourceValueLabel != null) {
            this.resourceValueLabel.text = value.ToString();
        }

        foreach (var modal in ModalManager.Instance.ActiveModals) {
            if (modal.ModalContent.Q<Label>("resource-current-value") is Label resourceValue) {
                resourceValue.text = value.ToString();
            }
        }
    }

    public void UpdateFundsValue(uint value) {
        if (this.fundsValueLabel != null) {
            this.fundsValueLabel.text = value.ToString();
        }

        foreach (var modal in ModalManager.Instance.ActiveModals) {
            if (modal.ModalContent.Q<Label>("funds-current-value") is Label fundsValue) {
                fundsValue.text = value.ToString();
            }
        }
    }

    public void UpdateEnvironmentValue(uint value) {
        if (this.enviromentBar != null) {
            this.enviromentBar.value = Mathf.Clamp(value, 0, 100);
            this.enviromentBar.title = $"{value} %";
        }
        if (this.enviromentValueLabel != null) {
            this.enviromentValueLabel.text = $"{value}/100";
        }
    }

    public void UpdateSocietyValue(uint value) {
        if (this.societyBar != null) {
            this.societyBar.value = Mathf.Clamp(value, 0, 100);
            this.societyBar.title = $"{value}/100";
        }
        if (this.societyValueLabel != null) {
            this.societyValueLabel.text = $"{value}/100";
        }
    }

    public void UpdateEconomyValue(uint value) {
        if (this.economyBar != null) {
            this.economyBar.value = Mathf.Clamp(value, 0, 100);
            this.economyBar.title = $"{value} %";
        }
        if (this.economyValueLabel != null) {
            this.economyValueLabel.text = $"{value}/100";
        }
    }
}
