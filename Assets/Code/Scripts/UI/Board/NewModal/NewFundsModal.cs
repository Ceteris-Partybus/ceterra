using Mirror;
using UnityEngine;
using UnityEngine.UIElements;

public class NewFundsModal : NewModal {
    protected override void Start() {
        this.visualTreeAsset = ModalMap.Instance.FundsModalTemplate;
        base.Start();
    }

    protected override void OnModalShown() {
        // Setup your specific modal logic here
        Button depositButton = modalElement.Q<Button>("funds-deposit-button");
        depositButton.clicked += OnDepositClicked;
    }

    [ClientCallback]
    private void OnDepositClicked() {
        BoardPlayer localPlayer = BoardContext.Instance.GetLocalPlayer();
        CmdRequestDeposit(10, localPlayer);
    }

    [Command(requiresAuthority = false)]
    private void CmdRequestDeposit(uint amount, BoardPlayer player) {
        player.RemoveCoins(amount);
        BoardContext.Instance.UpdateFundsStat(amount);
    }
}