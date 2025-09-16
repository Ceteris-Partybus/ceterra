using Mirror;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class NewFundsModal : NewModal {

    [SerializeField]
    private NewModal fundsHistoryModal;

    protected override void Start() {
        this.visualTreeAsset = ModalMap.Instance.FundsModalTemplate;
        base.Start();
        fundsHistoryModal = FindObjectsByType<NewFundsHistoryModal>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).FirstOrDefault();
    }

    protected override void OnModalShown() {
        // Setup your specific modal logic here
        Button depositButton = modalElement.Q<Button>("funds-deposit-button");
        depositButton.clicked += OnDepositClicked;
        Button fundsHistoryButton = modalElement.Q<Button>("funds-history-button");
        fundsHistoryButton.clicked += OnFundsHistoryClicked;
    }

    [ClientCallback]
    private void OnDepositClicked() {
        BoardPlayer localPlayer = BoardContext.Instance.GetLocalPlayer();
        CmdRequestDeposit(10, localPlayer);
    }

    [ClientCallback]
    private void OnFundsHistoryClicked() {
        NewModalManager.Instance.Show(fundsHistoryModal, false);
    }

    [Command(requiresAuthority = false)]
    private void CmdRequestDeposit(uint amount, BoardPlayer player) {
        player.RemoveCoins(amount);
        BoardContext.Instance.UpdateFundsStat(amount);
    }
}