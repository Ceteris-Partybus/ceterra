using UnityEngine;
using UnityEngine.UIElements;

public class FundsModal : Modal {

    private Button fundsHistoryButton;
    private Button investButton;
    private Button depositButton;
    private Label fundsValueLabel;
    private VisualTreeAsset fundsHistoryModalTemplate;
    private VisualTreeAsset fundsDepositModalTemplate;
    private VisualTreeAsset fundsInvestProposalModalTemplate;
    private VisualTreeAsset fundsInvestProposalSubmitModalTemplate;
    private VisualTreeAsset investProposalVoteModalTemplate;

    public FundsModal(VisualTreeAsset contentTemplate, VisualTreeAsset fundsHistoryModalTemplate, VisualTreeAsset fundsDepositModalTemplate, VisualTreeAsset fundsInvestProposalModalTemplate, VisualTreeAsset fundsInvestProposalSubmitModalTemplate, VisualTreeAsset investProposalVoteModalTemplate) : base(contentTemplate) {
        this.fundsHistoryModalTemplate = fundsHistoryModalTemplate;
        this.fundsDepositModalTemplate = fundsDepositModalTemplate;
        this.fundsInvestProposalModalTemplate = fundsInvestProposalModalTemplate;
        this.fundsInvestProposalSubmitModalTemplate = fundsInvestProposalSubmitModalTemplate;
        this.investProposalVoteModalTemplate = investProposalVoteModalTemplate;
    }

    protected override void InitializeContent() {
        this.fundsHistoryButton = modalContent.Q<Button>("funds-history-button");
        this.investButton = modalContent.Q<Button>("funds-invest-button");
        this.depositButton = modalContent.Q<Button>("funds-deposit-button");
        this.fundsValueLabel = modalContent.Q<Label>("funds-current-value");

        if (this.fundsHistoryButton != null) {
            this.fundsHistoryButton.clicked += OnFundsHistoryButtonClicked;
        }

        if (this.investButton != null) {
            this.investButton.clicked += OnInvestButtonClicked;
        }

        if (this.depositButton != null) {
            this.depositButton.clicked += OnDepositButtonClicked;
        }

        if (fundsValueLabel != null) {
            this.fundsValueLabel.text = BoardContext.Instance.FundsStat.ToString();
        }
    }

    private void OnFundsHistoryButtonClicked() {
        ModalManager.Instance.ShowModal(new FundsHistoryModal(this.fundsHistoryModalTemplate));
    }

    private void OnInvestButtonClicked() {
        ModalManager.Instance.ShowModal(new FundsInvestProposalModal(this.fundsInvestProposalModalTemplate, this.fundsInvestProposalSubmitModalTemplate, this.investProposalVoteModalTemplate));
    }

    private void OnDepositButtonClicked() {
        ModalManager.Instance.ShowModal(new FundsDepositModal(this.fundsDepositModalTemplate));
    }

    protected override void OnClose() {
        // Unregister events when modal is closed
        if (this.fundsHistoryButton != null) {
            this.fundsHistoryButton.clicked -= OnFundsHistoryButtonClicked;
        }
        if (this.investButton != null) {
            this.investButton.clicked -= OnInvestButtonClicked;
        }
        if (this.depositButton != null) {
            this.depositButton.clicked -= OnDepositButtonClicked;
        }
    }
}
