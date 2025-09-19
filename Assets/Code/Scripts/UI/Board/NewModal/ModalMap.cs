using UnityEngine;
using UnityEngine.UIElements;

public class ModalMap : NetworkedSingleton<ModalMap> {
    [Header("Funds")]
    [SerializeField] public VisualTreeAsset FundsModalTemplate;
    [SerializeField] public VisualTreeAsset FundsHistoryModalTemplate;
    [SerializeField] public VisualTreeAsset FundsDepositModalTemplate;
    [SerializeField] public VisualTreeAsset FundsInvestProposalModalTemplate;
    [SerializeField] public VisualTreeAsset FundsInvestProposalSubmitModalTemplate;
    [Header("Investments")]
    [SerializeField] public VisualTreeAsset InvestModalTemplate;
    [SerializeField] public VisualTreeAsset InvestDepositModalTemplate;
    [SerializeField] public VisualTreeAsset InvestProposalVoteModalTemplate;
    [Header("Resources")]
    [SerializeField] public VisualTreeAsset ResourceModalTemplate;
    [SerializeField] public VisualTreeAsset ResourceHistoryModalTemplate;
}