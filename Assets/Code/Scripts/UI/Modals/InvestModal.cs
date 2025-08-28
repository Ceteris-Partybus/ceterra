using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class InvestModal : Modal {
    private VisualTreeAsset fundsInvestProposalSubmitModalTemplate;
    private VisualTreeAsset investProposalVoteModalTemplate;
    private VisualTreeAsset investDepositModal;
    private List<VisualElement> investCards = new();
    public InvestModal(VisualTreeAsset contentTemplate, VisualTreeAsset fundsInvestProposalSubmitModalTemplate, VisualTreeAsset investProposalVoteModalTemplate, VisualTreeAsset investDepositModal) : base(contentTemplate) {
        this.fundsInvestProposalSubmitModalTemplate = fundsInvestProposalSubmitModalTemplate;
        this.investProposalVoteModalTemplate = investProposalVoteModalTemplate;
        this.investDepositModal = investDepositModal;
    }

    protected override void InitializeContent() {
        investCards = this.modalContent.Query<VisualElement>(name: "investments-grid").Children<VisualElement>().ToList();

        foreach (var investCard in investCards) {
            if (investCard != null) {
                Button depositButton = investCard.Q<Button>("invest-card-deposit-button");
                Button proposeInvestButton = investCard.Q<Button>("invest-card-propose-invest-button");
                if (depositButton != null) {
                    depositButton.clicked += () => {
                        this.OnInvestCardDepositButtonClicked(investCard);
                    };
                }
                if (proposeInvestButton != null) {
                    proposeInvestButton.clicked += () => {
                        this.OnInvestCardProposeInvestButtonClicked(investCard);
                    };
                }
            }
        }
    }

    private void OnInvestCardDepositButtonClicked(VisualElement investCard) {
        // Handle deposit button click
        Debug.Log("Deposit button clicked for invest card: " + investCard.name);
        // TODO: query investment ID from investCard
        var investmentIdentifier = "REPLACE_ME";
        ModalManager.Instance.ShowModal(new InvestDepositModal(this.investDepositModal, investmentIdentifier));
    }

    private void OnInvestCardProposeInvestButtonClicked(VisualElement investCard) {
        // Handle propose invest button click
        Debug.Log("Propose invest button clicked for invest card: " + investCard.name);
        ModalManager.Instance.ShowModal(new FundsInvestProposalSubmitModal(this.fundsInvestProposalSubmitModalTemplate, this.investProposalVoteModalTemplate));
    }

    protected override void OnClose() {
        foreach (var investCard in investCards) {
            if (investCard != null) {
                Button depositButton = investCard.Q<Button>("invest-card-deposit-button");
                Button proposeInvestButton = investCard.Q<Button>("invest-card-propose-invest-button");
                if (depositButton != null) {
                    depositButton.clicked -= () => {
                        this.OnInvestCardDepositButtonClicked(investCard);
                    };
                }
                if (proposeInvestButton != null) {
                    proposeInvestButton.clicked -= () => {
                        this.OnInvestCardProposeInvestButtonClicked(investCard);
                    };
                }
            }
        }
    }
}
