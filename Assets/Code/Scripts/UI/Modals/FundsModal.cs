using Mirror;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class FundsModal : Modal {

    public static FundsModal Instance => GetInstance<FundsModal>();

    Button depositButton;
    Button fundsHistoryButton;
    Button investButton;

    protected override void Start() {
        this.visualTreeAsset = ModalMap.Instance.FundsModalTemplate;
        base.Start();
    }

    protected override void OnModalShown() {
        // Setup your specific modal logic here
        depositButton = modalElement.Q<Button>("funds-deposit-button");
        depositButton.clicked += OnDepositClicked;
        fundsHistoryButton = modalElement.Q<Button>("funds-history-button");
        fundsHistoryButton.clicked += OnFundsHistoryClicked;
        investButton = modalElement.Q<Button>("funds-invest-button");
        investButton.clicked += OnInvestClicked;
    }

    [ClientCallback]
    private void OnDepositClicked() {
        // BoardPlayer localPlayer = BoardContext.Instance.GetLocalPlayer();
        // CmdRequestDeposit(10, localPlayer);
        ModalManager.Instance.Show(FundsDepositModal.Instance, false);
    }

    [ClientCallback]
    private void OnFundsHistoryClicked() {
        ModalManager.Instance.Show(FundsHistoryModal.Instance, false);
    }

    [ClientCallback]
    private void OnInvestClicked() {
        ModalManager.Instance.Show(InvestModal.Instance, false);
    }

    // [Command(requiresAuthority = false)]
    // private void CmdRequestDeposit(int amount, BoardPlayer player) {
    //     player.RemoveCoins(amount);
    //     BoardContext.Instance.UpdateFundsStat(amount);
    // }
}