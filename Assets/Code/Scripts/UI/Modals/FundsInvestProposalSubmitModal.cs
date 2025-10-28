using Mirror;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class FundsInvestProposalSubmitModal : Modal {

    public static FundsInvestProposalSubmitModal Instance => GetInstance<FundsInvestProposalSubmitModal>();

    private UnsignedIntegerField amountField;
    private Button addAllFundingButton;
    private Button submitProposalButton;
    private VisualElement fullyFinancedInfo;

    public int InvestmentId;

    protected override void Start() {
        this.visualTreeAsset = ModalMap.Instance.FundsInvestProposalSubmitModalTemplate;
        base.Start();
    }

    protected override void OnModalShown() {
        this.amountField = modalElement.Q<UnsignedIntegerField>("investment-proposal-value");
        this.addAllFundingButton = modalElement.Q<Button>("add-all-funding-button");
        this.submitProposalButton = modalElement.Q<Button>("propose-investment-button");
        this.fullyFinancedInfo = modalElement.Q<VisualElement>("fully-financed-info");

        if (this.submitProposalButton != null) {
            this.submitProposalButton.clicked += OnSubmitProposalButtonClicked;
        }

        Investment investment = BoardContext.Instance.investments.FirstOrDefault(inv => inv.id == InvestmentId);

        if (investment.fullyFinanced) {
            amountField.value = 0;
            amountField.isReadOnly = true;
            fullyFinancedInfo.style.display = DisplayStyle.Flex;
            addAllFundingButton.style.display = DisplayStyle.None;
        }
        else {
            addAllFundingButton.clicked += OnAddAllFundingButtonClicked;
        }

        amountField.RegisterCallback<ChangeEvent<int>>(evt => {
            Investment investment = BoardContext.Instance.investments.FirstOrDefault(inv => inv.id == InvestmentId);
            int totalFunds = BoardContext.Instance.FundsStat;
            int requiredFunds = investment.requiredMoney - investment.currentMoney;
            if (evt.newValue > requiredFunds || evt.newValue > totalFunds) {
                amountField.value = (uint)Mathf.Min(requiredFunds, totalFunds);
            }
        });
    }

    [ClientCallback]
    private void OnAddAllFundingButtonClicked() {
        Audiomanager.Instance?.PlayClickSound();
        Investment investment = BoardContext.Instance.investments.FirstOrDefault(inv => inv.id == InvestmentId);
        int requiredFunds = investment.requiredMoney - investment.currentMoney;

        if (requiredFunds > BoardContext.Instance.FundsStat) {
            InfoModal.Instance.Message = LocalizationManager.Instance.GetLocalizedText(56640685331546112);
            ModalManager.Instance.Show(InfoModal.Instance);
            return;
        }
        this.amountField.value = (uint)requiredFunds;
    }

    [ClientCallback]
    private void OnSubmitProposalButtonClicked() {
        Audiomanager.Instance?.PlayClickSound();
        Investment investment = BoardContext.Instance.investments.FirstOrDefault(inv => inv.id == InvestmentId);
        int requiredResources = investment.requiredResources;

        bool proposesFullAmount = amountField.value + investment.currentMoney >= investment.requiredMoney;

        if (BoardContext.Instance.ResourceStat < requiredResources && proposesFullAmount) {
            InfoModal.Instance.Message = LocalizationManager.Instance.GetLocalizedText(56640685331546113);
            ModalManager.Instance.Show(InfoModal.Instance);
            return;
        }

        if (amountField.value == 0 && !investment.fullyFinanced) {
            InfoModal.Instance.Message = LocalizationManager.Instance.GetLocalizedText(56640685331546114);
            ModalManager.Instance.Show(InfoModal.Instance);
            return;
        }

        int playerId = BoardContext.Instance.GetLocalPlayer().PlayerId;
        CmdSetVoteProperties(InvestmentId, playerId, (int)amountField.value);
    }

    [Command(requiresAuthority = false)]
    private void CmdSetVoteProperties(int investmentId, int playerId, int proposedCoins) {
        RpcSetVoteProperties(investmentId, playerId, proposedCoins);
    }

    [ClientRpc]
    private void RpcSetVoteProperties(int investmentId, int playerId, int proposedCoins) {
        InvestProposalVoteModal.Instance.InvestmentId = investmentId;
        InvestProposalVoteModal.Instance.PlayerId = playerId;
        InvestProposalVoteModal.Instance.ProposedCoins = proposedCoins;
        ModalManager.Instance.Show(InvestProposalVoteModal.Instance);
    }
}