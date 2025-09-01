using UnityEngine;
using UnityEngine.UIElements;

public class InvestDepositModal : Modal {

    private Button depositSubmitButton;
    private UnsignedIntegerField depositValueField;
    private Button depositAdd10Button;
    private Button depositAdd100Button;
    private Button depositAdd1000Button;
    private object investmentIdentifier;

    public InvestDepositModal(VisualTreeAsset contentTemplate, object investmentIdentifier) : base(contentTemplate) {
        this.investmentIdentifier = investmentIdentifier;
    }

    protected override void InitializeContent() {
        this.depositSubmitButton = this.modalContent.Q<Button>("deposit-submit-button");
        this.depositValueField = this.modalContent.Q<UnsignedIntegerField>("deposit-value");
        this.depositAdd10Button = this.modalContent.Q<Button>("deposit-add-10");
        this.depositAdd100Button = this.modalContent.Q<Button>("deposit-add-100");
        this.depositAdd1000Button = this.modalContent.Q<Button>("deposit-add-1000");

        if (this.depositSubmitButton != null) {
            this.depositSubmitButton.clicked += this.OnDepositSubmitButtonClicked;
        }

        if (this.depositAdd10Button != null) {
            this.depositAdd10Button.clicked += this.OnDepositAdd10ButtonClicked;
        }

        if (this.depositAdd100Button != null) {
            this.depositAdd100Button.clicked += this.OnDepositAdd100ButtonClicked;
        }

        if (this.depositAdd1000Button != null) {
            this.depositAdd1000Button.clicked += this.OnDepositAdd1000ButtonClicked;
        }
    }

    private void OnDepositSubmitButtonClicked() {
        Debug.Log("Deposit submit button clicked!");
        // Handle deposit submission logic here
        uint depositValue = this.depositValueField.value;
        Debug.Log($"Deposit value: {depositValue}");
    }

    private void OnDepositAdd10ButtonClicked() {
        this.OnDepositAddButtonClicked(10);
    }

    private void OnDepositAdd100ButtonClicked() {
        this.OnDepositAddButtonClicked(100);
    }

    private void OnDepositAdd1000ButtonClicked() {
        this.OnDepositAddButtonClicked(1000);
    }

    private void OnDepositAddButtonClicked(uint amount) {
        if (this.depositValueField != null) {
            this.depositValueField.value += amount;
        }
    }

    protected override void OnClose() {
        // Unregister events when modal is closed
        if (this.depositSubmitButton != null) {
            this.depositSubmitButton.clicked -= this.OnDepositSubmitButtonClicked;
        }
        if (this.depositAdd10Button != null) {
            this.depositAdd10Button.clicked -= this.OnDepositAdd10ButtonClicked;
        }
        if (this.depositAdd100Button != null) {
            this.depositAdd100Button.clicked -= this.OnDepositAdd100ButtonClicked;
        }
        if (this.depositAdd1000Button != null) {
            this.depositAdd1000Button.clicked -= this.OnDepositAdd1000ButtonClicked;
        }
    }
}