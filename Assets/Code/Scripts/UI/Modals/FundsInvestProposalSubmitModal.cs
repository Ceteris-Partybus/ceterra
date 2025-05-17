using UnityEngine;
using UnityEngine.UIElements;

public class FundsInvestProposalSubmitModal : Modal {
    private UnsignedIntegerField amountField;
    private Button addAllFundingButton;
    private Button submitProposalButton;
    private VisualTreeAsset investProposalVoteModalTemplate;

    public FundsInvestProposalSubmitModal(VisualTreeAsset contentTemplate, VisualTreeAsset investProposalVoteModalTemplate) : base(contentTemplate) {
        this.investProposalVoteModalTemplate = investProposalVoteModalTemplate;
    }

    protected override void InitializeContent() {
        this.amountField = modalContent.Q<UnsignedIntegerField>("investment-proposal-value");
        this.addAllFundingButton = modalContent.Q<Button>("add-all-funding-button");
        this.submitProposalButton = modalContent.Q<Button>("propose-investment-button");

        if (this.addAllFundingButton != null) {
            this.addAllFundingButton.clicked += OnAddAllFundingButtonClicked;
        }

        if (this.submitProposalButton != null) {
            this.submitProposalButton.clicked += OnSubmitProposalButtonClicked;
        }
    }

    private void OnAddAllFundingButtonClicked() {
        // Get available funds, check progress in funds for current investment
        // Set the amount field to the available funds
        Debug.Log("Add all funding button clicked");
    }

    private void OnSubmitProposalButtonClicked() {
        Debug.Log("Investment proposal submitted with amount: " + this.amountField.value);
        // This is just a placeholder, the vote should be submitted to the server and then shown at a fitting time.
        ModalManager.Instance.ShowModal(new InvestProposalVoteModal(this.investProposalVoteModalTemplate));
    }
}