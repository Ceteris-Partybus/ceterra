using Mirror;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class FundsDepositModal : Modal {

    public static FundsDepositModal Instance => GetInstance<FundsDepositModal>();

    private Button depositSubmitButton;
    private UnsignedIntegerField depositValueField;
    private Button depositAdd10Button;
    private Button depositAdd100Button;
    private Button depositAdd1000Button;

    protected override void Start() {
        this.visualTreeAsset = ModalMap.Instance.FundsDepositModalTemplate;
        base.Start();
    }

    protected override void OnModalShown() {
        this.depositSubmitButton = modalElement.Q<Button>("deposit-submit-button");
        this.depositValueField = modalElement.Q<UnsignedIntegerField>("deposit-value");
        this.depositAdd10Button = modalElement.Q<Button>("deposit-add-10");
        this.depositAdd100Button = modalElement.Q<Button>("deposit-add-100");
        this.depositAdd1000Button = modalElement.Q<Button>("deposit-add-1000");

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

    [ClientCallback]
    private void OnDepositSubmitButtonClicked() {
        int depositValue = (int)this.depositValueField.value;
        var localPlayer = BoardContext.Instance.GetLocalPlayer();

        if (depositValue <= 0) {
            ErrorModal.Instance.Message = "Der Einzahlungsbetrag muss größer als 0 sein.";
            ModalManager.Instance.Show(ErrorModal.Instance);
            return;
        }

        if (localPlayer.Coins < depositValue) {
            ErrorModal.Instance.Message = "Du besitzt nicht genügend Münzen.";
            ModalManager.Instance.Show(ErrorModal.Instance);
            return;
        }

        CmdDepositFunds(depositValue, localPlayer);
        ModalManager.Instance.Hide();
    }

    [Command(requiresAuthority = false)]
    private void CmdDepositFunds(int depositValue, BoardPlayer localPlayer) {
        FundsHistoryEntry entry = new FundsHistoryEntry(depositValue, HistoryEntryType.DEPOSIT, "Einzahlung von " + localPlayer.PlayerName);
        BoardContext.Instance.fundsHistory.Add(entry);
        BoardContext.Instance.UpdateFundsStat(depositValue);
        localPlayer.RemoveCoins(depositValue);
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

    private void OnDepositAddButtonClicked(int amount) {
        if (this.depositValueField != null) {
            this.depositValueField.value += (uint)amount;
        }
    }

    protected override void OnModalHidden() {
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