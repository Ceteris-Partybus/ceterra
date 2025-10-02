using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class BoardOverlay : NetworkedSingleton<BoardOverlay> {

    [SerializeField] private UIDocument uiDocument;

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
        var clientPlayerCard = rootElement.Q<TemplateContainer>("client-player-card")?
            .Q<VisualElement>("player-card");
        this.playerNameLabel = clientPlayerCard.Q<Label>("player-card__display-name");
        var healthBarContainer = clientPlayerCard.Q<TemplateContainer>("player-card__health-bar");
        this.playerHealthBar = healthBarContainer.Children().OfType<ProgressBar>().FirstOrDefault();
        this.playerHealthValue = clientPlayerCard.Q<Label>("player-card__health-value");
        this.playerCoinsValue = rootElement.Q<Label>("player-card__coins-value");
        this.playerOverview = rootElement.Q<VisualElement>("player-overview");
        this.resourcesButton = rootElement.Q<Button>("ui-buttons__resources");
        this.fundsButton = rootElement.Q<Button>("ui-buttons__funds");
        this.investButton = rootElement.Q<Button>("ui-buttons__invest");
        this.resourceValueLabel = rootElement.Q<Label>("resources-value");
        this.fundsValueLabel = rootElement.Q<Label>("funds-value");
        var enviromentBarContainer = rootElement.Q<TemplateContainer>("environment-bar");
        this.enviromentBar = enviromentBarContainer.Children().OfType<ProgressBar>().FirstOrDefault();
        this.enviromentValueLabel = rootElement.Q<Label>("environment-bar-value");
        var societyBarContainer = rootElement.Q<TemplateContainer>("society-bar");
        this.societyBar = societyBarContainer.Children().OfType<ProgressBar>().FirstOrDefault();
        this.societyValueLabel = rootElement.Q<Label>("society-bar-value");
        var economyBarContainer = rootElement.Q<TemplateContainer>("economy-bar");
        this.economyBar = economyBarContainer.Children().OfType<ProgressBar>().FirstOrDefault();
        this.economyValueLabel = rootElement.Q<Label>("economy-bar-value");

        resourcesButton.clicked += () => {
            ModalManager.Instance.Show(ResourceModal.Instance);
        };

        fundsButton.clicked += () => {
            ModalManager.Instance.Show(FundsModal.Instance);
        };

        investButton.clicked += () => {
            ModalManager.Instance.Show(InvestModal.Instance);
        };
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

    [ClientCallback]
    public void UpdateLocalPlayerName(string playerName) {
        this.playerNameLabel.text = playerName;
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
        FillModalsWithCurrentValues();
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
        FillModalsWithCurrentValues();
    }

    public void UpdateFundsValue(uint value) {
        if (this.fundsValueLabel != null) {
            this.fundsValueLabel.text = value.ToString();
        }
        FillModalsWithCurrentValues();
    }

    public void FillModalsWithCurrentValues() {
        foreach (var modal in ModalManager.Instance.ModalStack) {

            if (modal == null || modal.ModalElement == null) {
                continue;
            }

            if (modal.ModalElement.Q<Label>("funds-current-value") is Label fundsValue) {
                fundsValue.text = BoardContext.Instance.FundsStat.ToString();
            }
            if (modal.ModalElement.Q<Label>("resource-current-value") is Label resourceValue) {
                resourceValue.text = BoardContext.Instance.ResourceStat.ToString();
            }
            if (modal.ModalElement.Q<Label>("coins-current-value") is Label coinsValue) {
                coinsValue.text = BoardContext.Instance.GetLocalPlayer().Coins.ToString();
            }
        }
    }

    public void RecalculateInvestment(int investmentId) {
        StartCoroutine(DelayedRecalculateInvestment(investmentId));
    }

    private IEnumerator DelayedRecalculateInvestment(int investmentId) {
        yield return new WaitForEndOfFrame();

        var investModal = InvestModal.Instance;

        if (investModal.IsVisible() == false) {
            yield break;
        }

        if (investModal != null) {
            investModal.Recalculate(investmentId);
        }
    }

    public void UpdateFundsHistory(FundsHistoryEntry entry) {
        if (FundsHistoryModal.Instance.IsVisible()) {
            FundsHistoryModal.Instance.AddEntryToTop(entry);
        }
    }

    public void UpdateResourceHistory(ResourceHistoryEntry entry) {
        if (ResourceHistoryModal.Instance.IsVisible()) {
            ResourceHistoryModal.Instance.AddEntryToTop(entry);
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

    [ClientCallback]
    public void UpdateResourcesNextRoundValue() {
        if (ResourceModal.Instance?.IsVisible() ?? false) {
            ResourceModal.Instance.Refresh();
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

    public void UpdateTrend(string label, int lastValue, int newValue) {
        Label trendLabel = rootElement.Q<Label>(label);
        trendLabel.ClearClassList();
        trendLabel.AddToClassList(GetTrendClass(lastValue, newValue));
    }

    private string GetTrendClass(int lastValue, int newValue) {
        if (newValue > lastValue) {
            return "trend-rising";
        }
        else if (newValue < lastValue) {
            return "trend-falling";
        }
        else {
            return "trend-neutral";
        }
    }
}
