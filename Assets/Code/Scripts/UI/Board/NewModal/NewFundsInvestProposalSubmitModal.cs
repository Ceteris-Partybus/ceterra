using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class NewFundsInvestProposalSubmitModal : NewModal {

    private UnsignedIntegerField amountField;
    private Button addAllFundingButton;
    private Button submitProposalButton;

    protected override void Start() {
        this.visualTreeAsset = ModalMap.Instance.FundsInvestProposalSubmitModalTemplate;
        base.Start();
    }

    protected override void OnModalShown() {
        this.amountField = modalElement.Q<UnsignedIntegerField>("investment-proposal-value");
        this.addAllFundingButton = modalElement.Q<Button>("add-all-funding-button");
        this.submitProposalButton = modalElement.Q<Button>("propose-investment-button");

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
        NewModalManager.Instance.Show(FindObjectsByType<NewInvestProposalVoteModal>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).FirstOrDefault(), true);
    }
}