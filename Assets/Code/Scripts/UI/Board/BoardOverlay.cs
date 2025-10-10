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
        rootElement = uiDocument.rootVisualElement;
        var clientPlayerCard = rootElement.Q<TemplateContainer>("client-player-card")?
            .Q<VisualElement>("player-card");
        playerNameLabel = clientPlayerCard.Q<Label>("player-card__display-name");
        var healthBarContainer = clientPlayerCard.Q<TemplateContainer>("player-card__health-bar");
        playerHealthBar = healthBarContainer.Children().OfType<ProgressBar>().FirstOrDefault();
        playerHealthValue = clientPlayerCard.Q<Label>("player-card__health-value");
        playerCoinsValue = rootElement.Q<Label>("player-card__coins-value");
        playerOverview = rootElement.Q<VisualElement>("player-overview");
        resourcesButton = rootElement.Q<Button>("ui-buttons__resources");
        fundsButton = rootElement.Q<Button>("ui-buttons__funds");
        investButton = rootElement.Q<Button>("ui-buttons__invest");
        resourceValueLabel = rootElement.Q<Label>("resources-value");
        fundsValueLabel = rootElement.Q<Label>("funds-value");
        var enviromentBarContainer = rootElement.Q<TemplateContainer>("environment-bar");
        enviromentBar = enviromentBarContainer.Children().OfType<ProgressBar>().FirstOrDefault();
        enviromentValueLabel = rootElement.Q<Label>("environment-bar-value");
        var societyBarContainer = rootElement.Q<TemplateContainer>("society-bar");
        societyBar = societyBarContainer.Children().OfType<ProgressBar>().FirstOrDefault();
        societyValueLabel = rootElement.Q<Label>("society-bar-value");
        var economyBarContainer = rootElement.Q<TemplateContainer>("economy-bar");
        economyBar = economyBarContainer.Children().OfType<ProgressBar>().FirstOrDefault();
        economyValueLabel = rootElement.Q<Label>("economy-bar-value");

        resourcesButton.clicked += () => ModalManager.Instance.Show(ResourceModal.Instance);
        fundsButton.clicked += () => ModalManager.Instance.Show(FundsModal.Instance);
        investButton.clicked += () => ModalManager.Instance.Show(InvestModal.Instance);
    }

    [ClientCallback]
    protected override void Start() {
        base.Start();

        UpdateFundsValue(BoardContext.Instance.FundsStat);
        UpdateResourceValue(BoardContext.Instance.ResourceStat);
        UpdateEconomyValue(BoardContext.Instance.EconomyStat);
        UpdateSocietyValue(BoardContext.Instance.SocietyStat);
        UpdateEnvironmentValue(BoardContext.Instance.EnvironmentStat);
        UpdateTrends();

        PlayerStats localPlayerStats = BoardContext.Instance.GetLocalPlayer().PlayerStats;
        localPlayerStats.OnCoinsUpdated += UpdateLocalPlayerCoins;
        localPlayerStats.OnHealthUpdated += UpdateLocalPlayerHealth;

        foreach (var player in BoardContext.Instance.GetRemotePlayers()) {
            player.PlayerStats.OnCoinsUpdated += (newCoins) => UpdateRemotePlayerCoins(newCoins, player.PlayerId);
            player.PlayerStats.OnHealthUpdated += (newHealth) => UpdateRemotePlayerHealth(newHealth, player.PlayerId);
        }

        // Call Update* methods to initialize UI
        UpdateLocalPlayerCoins(localPlayerStats.GetCoins());
        UpdateLocalPlayerHealth(localPlayerStats.GetHealth());
        foreach (var player in BoardContext.Instance.GetRemotePlayers()) {
            UpdateRemotePlayerCoins(player.PlayerStats.GetCoins(), player.PlayerId);
            UpdateRemotePlayerHealth(player.PlayerStats.GetHealth(), player.PlayerId);
        }
    }

    public bool IsPlayerAdded(int playerId) {
        return playerElements.ContainsKey(playerId);
    }

    public void AddPlayer(BoardPlayer player) {
        var playerElement = playerCardTemplate.CloneTree();
        var card = playerElement.Q<VisualElement>("player-card");
        var playerNameLabel = card.Q<Label>("player-card__display-name");
        var healthBarContainer = card.Q<TemplateContainer>("player-card__health-bar");

        var playerHealthBar = healthBarContainer.Children().OfType<ProgressBar>().FirstOrDefault();
        playerHealthBar.value = Constants.MAX_HEALTH;
        playerHealthBar.title = $"{Constants.MAX_HEALTH} / {Constants.MAX_HEALTH}";
        playerHealthBar.ClearClassList();
        playerHealthBar.AddToClassList(this.GetHealthBarClassName(Constants.MAX_HEALTH));

        var healthBarLabel = card.Q<Label>("player-card__health-value");
        healthBarLabel.text = $"{player.PlayerStats.GetHealth()} / {Constants.MAX_HEALTH}";

        var coinsLabel = card.Q<Label>("player-card__coins-value");
        coinsLabel.text = player.PlayerStats.GetCoins().ToString();

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

    private string GetHealthBarClassName(int health) {
        if (health > 70) {
            return "health-bar--green";
        }
        if (health > 35) {
            return "health-bar--yellow";
        }
        return "health-bar--red";
    }

    [ClientCallback]
    public void UpdateLocalPlayerName(string playerName) {
        this.playerNameLabel.text = playerName;
    }

    public void UpdateLocalPlayerHealth(int newHealth) {
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

    public void UpdateRemotePlayerHealth(int newHealth, int playerId) {
        if (playerElements.TryGetValue(playerId, out var playerCard)) {
            var healthBarContainer = playerCard.Q<TemplateContainer>("player-card__health-bar");
            var otherPlayerHealthbar = healthBarContainer.Children().OfType<ProgressBar>().FirstOrDefault();
            otherPlayerHealthbar.value = newHealth;
            otherPlayerHealthbar.title = $"{newHealth} / 100";
            otherPlayerHealthbar.ClearClassList();
            otherPlayerHealthbar.AddToClassList(this.GetHealthBarClassName(newHealth));
            var healthTextLabel = playerCard.Q<Label>("player-card__health-value");
            healthTextLabel.text = $"{newHealth} / 100";
        }
    }

    public void UpdateLocalPlayerCoins(int newCoins) {
        this.playerCoinsValue.text = newCoins.ToString();
        FillModalsWithCurrentValues();
    }

    public void UpdateRemotePlayerCoins(int newCoins, int playerId) {
        if (playerElements.TryGetValue(playerId, out var playerCard)) {
            var playerCoinsLabel = playerCard.Q<Label>("player-card__coins-value");
            playerCoinsLabel.text = newCoins.ToString();
        }
    }

    public void UpdateResourceValue(int value) {
        this.resourceValueLabel.text = value.ToString();

        FillModalsWithCurrentValues();
    }

    public void UpdateFundsValue(int value) {
        this.fundsValueLabel.text = value.ToString();
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
                coinsValue.text = BoardContext.Instance.GetLocalPlayer().PlayerStats.GetCoins().ToString();
            }
        }
    }

    public void RecalculateInvestment(int investmentId) {
        StartCoroutine(DelayedRecalculateInvestment(investmentId));
    }

    private IEnumerator DelayedRecalculateInvestment(int investmentId) {
        yield return new WaitForEndOfFrame();

        var investModal = InvestModal.Instance;

        if (!investModal.IsVisible()) {
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

    public void UpdateEnvironmentValue(int value) {
        this.enviromentBar.value = Mathf.Clamp(value, 0, 100);
        this.enviromentBar.title = $"{value} %";
        this.enviromentValueLabel.text = $"{value}/100";
    }

    [ClientCallback]
    public void UpdateResourcesNextRoundValue() {
        if (ResourceModal.Instance?.IsVisible() ?? false) {
            ResourceModal.Instance.Refresh();
        }
    }

    public void UpdateSocietyValue(int value) {
        this.societyBar.value = Mathf.Clamp(value, 0, 100);
        this.societyBar.title = $"{value}/100";
        this.societyValueLabel.text = $"{value}/100";
    }

    public void UpdateEconomyValue(int value) {
        this.economyBar.value = Mathf.Clamp(value, 0, 100);
        this.economyBar.title = $"{value} %";
        this.economyValueLabel.text = $"{value}/100";
    }

    public void UpdateTrends() {
        UpdateTrend("trend-economy", GetTrendClass(BoardContext.Instance.EconomyTrend));
        UpdateTrend("trend-society", GetTrendClass(BoardContext.Instance.SocietyTrend));
        UpdateTrend("trend-environment", GetTrendClass(BoardContext.Instance.EnvironmentTrend));
    }

    private void UpdateTrend(string label, string className) {
        Label trendLabel = rootElement.Q<Label>(label);
        trendLabel.ClearClassList();
        trendLabel.AddToClassList(className);
    }

    private string GetTrendClass(Trend trend) {
        if (trend == Trend.RISING) {
            return "trend-rising";
        }
        if (trend == Trend.FALLING) {
            return "trend-falling";
        }
        return "trend-neutral";
    }
}
