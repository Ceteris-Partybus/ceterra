using Mirror;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class NewFundsModal : NewModal {

    [SerializeField]
    private NewModal fundsHistoryModal;
    [SerializeField]
    private NewModal investProposalModal;
    [SerializeField]
    private NewModal fundsDepositModal;

    Button depositButton;
    Button fundsHistoryButton;
    Button investButton;

    protected override void Start() {
        this.visualTreeAsset = ModalMap.Instance.FundsModalTemplate;
        base.Start();
        fundsHistoryModal = FindObjectsByType<NewFundsHistoryModal>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).FirstOrDefault();
        investProposalModal = FindObjectsByType<NewFundsInvestProposalModal>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).FirstOrDefault();
        fundsDepositModal = FindObjectsByType<NewFundsDepositModal>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).FirstOrDefault();
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
        NewModalManager.Instance.Show(fundsDepositModal, false);
    }

    [ClientCallback]
    private void OnFundsHistoryClicked() {
        NewModalManager.Instance.Show(fundsHistoryModal, false);
    }

    [ClientCallback]
    private void OnInvestClicked() {
        NewModalManager.Instance.Show(investProposalModal, false);
    }

    // [Command(requiresAuthority = false)]
    // private void CmdRequestDeposit(uint amount, BoardPlayer player) {
    //     player.RemoveCoins(amount);
    //     BoardContext.Instance.UpdateFundsStat(amount);
    // }
}