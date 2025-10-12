using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class InvestModal : Modal {

    public static InvestModal Instance => GetInstance<InvestModal>();

    private List<VisualElement> investCards = new();

    [SerializeField]
    private VisualTreeAsset investmentCardTemplate;

    private Tab allTab;
    private Tab economyTab;
    private Tab societyTab;
    private Tab environmentTab;

    protected override void Start() {
        this.visualTreeAsset = ModalMap.Instance.InvestModalTemplate;
        base.Start();
    }

    protected override void OnModalShown() {
        allTab = modalElement.Q<Tab>("investment-tab-all");
        economyTab = modalElement.Q<Tab>("investment-tab-economy");
        societyTab = modalElement.Q<Tab>("investment-tab-society");
        environmentTab = modalElement.Q<Tab>("investment-tab-environment");

        EnsureTabGridExists(allTab);
        EnsureTabGridExists(economyTab);
        EnsureTabGridExists(societyTab);
        EnsureTabGridExists(environmentTab);

        ClearAllTabGrids();
        investCards.Clear();

        PopulateInvestmentTabs();
    }

    private void EnsureTabGridExists(Tab tab) {
        if (tab?.contentContainer.Q<VisualElement>("investments-grid") == null) {
            var grid = new VisualElement();
            grid.name = "investments-grid";
            grid.AddToClassList("investments-grid");
            tab?.contentContainer.Add(grid);
        }
    }

    private void ClearAllTabGrids() {
        allTab?.contentContainer.Q<VisualElement>("investments-grid")?.Clear();
        economyTab?.contentContainer.Q<VisualElement>("investments-grid")?.Clear();
        societyTab?.contentContainer.Q<VisualElement>("investments-grid")?.Clear();
        environmentTab?.contentContainer.Q<VisualElement>("investments-grid")?.Clear();
    }

    private void PopulateInvestmentTabs() {
        var allGrid = allTab?.contentContainer.Q<VisualElement>("investments-grid");
        var economyGrid = economyTab?.contentContainer.Q<VisualElement>("investments-grid");
        var societyGrid = societyTab?.contentContainer.Q<VisualElement>("investments-grid");
        var environmentGrid = environmentTab?.contentContainer.Q<VisualElement>("investments-grid");

        var investments = BoardContext.Instance.investments.OrderBy(inv => inv.completed).ToList();

        foreach (var investment in investments) {
            VisualElement investCard = CreateInvestmentCard(investment);
            allGrid?.Add(investCard);
            investCards.Add(investCard);

            switch (investment.type) {
                case InvestmentType.ECONOMY:
                    var economyCard = CreateInvestmentCard(investment);
                    economyGrid?.Add(economyCard);
                    break;
                case InvestmentType.SOCIETY:
                    var societyCard = CreateInvestmentCard(investment);
                    societyGrid?.Add(societyCard);
                    break;
                case InvestmentType.ENVIRONMENT:
                    var environmentCard = CreateInvestmentCard(investment);
                    environmentGrid?.Add(environmentCard);
                    break;
            }
        }
    }

    private VisualElement CreateInvestmentCard(Investment investment) {

        VisualElement investCard = investmentCardTemplate.Instantiate();
        investCard.name = $"investment-card-{investment.id}";
        investCard.Q<Label>("invest-card-title").text = LocalizationManager.Instance.GetLocalizedText(investment.displayName);
        investCard.Q<Label>("invest-card-description").text = LocalizationManager.Instance.GetLocalizedText(investment.description);
        investCard.Q<Label>("invest-card-required-money").text = investment.requiredMoney.ToString();
        investCard.Q<Label>("invest-card-required-resources").text = investment.requiredResources.ToString();
        investCard.Q<Label>("invest-card-cooldown").text = investment.cooldown.ToString();

        if (investment.inConstruction) {
            investCard.AddToClassList("in-construction");
            investCard.Q<Label>("construction-status-info").text = $"Im Aufbau, {investment.cooldown} Runde(n) verbleibend";
        }
        else if (investment.completed) {
            investCard.AddToClassList("completion");
            investCard.Q<Label>("completion-status-info").text = "Fertiggestellt";
        }
        else {
            investCard.RemoveFromClassList("in-construction");
            investCard.RemoveFromClassList("completion");
            investCard.Q<Label>("construction-status-info").text = "";
            investCard.Q<Label>("completion-status-info").text = "";
        }

        TemplateContainer moneyProgressBarContainer = investCard.Q<TemplateContainer>("invest-card-money-progress-bar");
        ProgressBar moneyProgressBar = moneyProgressBarContainer?.Children().OfType<ProgressBar>().FirstOrDefault();

        if (moneyProgressBar != null) {
            moneyProgressBar.value = investment.currentMoney / (float)investment.requiredMoney * 100;
        }

        Button depositButton = investCard.Q<Button>("invest-card-deposit-button");
        Button proposeInvestButton = investCard.Q<Button>("invest-card-propose-invest-button");
        int investmentId = investment.id;

        if (investment.inConstruction || investment.completed) {
            depositButton?.SetEnabled(false);
            proposeInvestButton?.SetEnabled(false);
        }
        else {
            depositButton?.SetEnabled(true);
            proposeInvestButton?.SetEnabled(true);
        }

        if (!investment.inConstruction && !investment.completed && !investment.fullyFinanced) {
            depositButton.clicked += () => {
                this.OnInvestCardDepositButtonClicked(investmentId);
            };
        }
        if (!investment.inConstruction && !investment.completed) {
            proposeInvestButton.clicked += () => {
                this.OnInvestCardProposeInvestButtonClicked(investmentId);
            };
        }

        return investCard;
    }

    public void Recalculate(int investmentId) {
        var investment = BoardContext.Instance.investments.FirstOrDefault(inv => inv.id == investmentId);
        if (investment == null) {
            Debug.LogWarning($"Investment with ID {investmentId} not found.");
            return;
        }

        UpdateInvestmentCardInTab(allTab, investmentId, investment);

        switch (investment.type) {
            case InvestmentType.ECONOMY:
                UpdateInvestmentCardInTab(economyTab, investmentId, investment);
                break;
            case InvestmentType.SOCIETY:
                UpdateInvestmentCardInTab(societyTab, investmentId, investment);
                break;
            case InvestmentType.ENVIRONMENT:
                UpdateInvestmentCardInTab(environmentTab, investmentId, investment);
                break;
        }

        Debug.Log($"Recalculated invest card with ID {investmentId}.");
    }

    private void UpdateInvestmentCardInTab(Tab tab, int investmentId, Investment investment) {
        var investCard = tab?.contentContainer.Q<VisualElement>($"investment-card-{investmentId}");
        if (investCard == null) {
            return;
        }

        investCard.Q<Label>("invest-card-title").text = LocalizationManager.Instance.GetLocalizedText(investment.displayName);
        investCard.Q<Label>("invest-card-description").text = LocalizationManager.Instance.GetLocalizedText(investment.description);
        investCard.Q<Label>("invest-card-required-money").text = investment.requiredMoney.ToString();
        investCard.Q<Label>("invest-card-required-resources").text = investment.requiredResources.ToString();
        investCard.Q<Label>("invest-card-cooldown").text = investment.cooldown.ToString();

        Button depositButton = investCard.Q<Button>("invest-card-deposit-button");
        Button proposeInvestButton = investCard.Q<Button>("invest-card-propose-invest-button");

        if (investment.inConstruction || investment.completed) {
            depositButton?.SetEnabled(false);
            proposeInvestButton?.SetEnabled(false);
        }
        else {
            if (investment.fullyFinanced) {
                depositButton?.SetEnabled(false);
            }
            proposeInvestButton?.SetEnabled(true);
        }

        if (investment.inConstruction) {
            investCard.AddToClassList("in-construction");
            investCard.Q<Label>("construction-status-info").text = $"Im Aufbau, {investment.cooldown} Runde(n) verbleibend";
        }
        else if (investment.completed) {
            investCard.AddToClassList("completion");
            investCard.Q<Label>("completion-status-info").text = "Fertiggestellt";
        }
        else {
            investCard.RemoveFromClassList("in-construction");
            investCard.RemoveFromClassList("completion");
            investCard.Q<Label>("construction-status-info").text = "";
            investCard.Q<Label>("completion-status-info").text = "";
        }

        TemplateContainer moneyProgressBarContainer = investCard.Q<TemplateContainer>("invest-card-money-progress-bar");
        ProgressBar moneyProgressBar = moneyProgressBarContainer?.Children().OfType<ProgressBar>().FirstOrDefault();
        if (moneyProgressBar != null) {
            moneyProgressBar.value = investment.currentMoney / (float)investment.requiredMoney * 100;
        }
    }

    private void OnInvestCardDepositButtonClicked(int investmentId) {
        InvestDepositModal.Instance.InvestmentId = investmentId;
        ModalManager.Instance.Show(InvestDepositModal.Instance);
    }

    private void OnInvestCardProposeInvestButtonClicked(int investmentId) {
        FundsInvestProposalSubmitModal.Instance.InvestmentId = investmentId;
        ModalManager.Instance.Show(FundsInvestProposalSubmitModal.Instance);
    }
}