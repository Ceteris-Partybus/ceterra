using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class FundsInvestProposalModal : Modal {

    private List<Button> investCards = new();
    private VisualTreeAsset fundsInvestProposalSubmitModalTemplate;
    private VisualTreeAsset investProposalVoteModalTemplate;

    public FundsInvestProposalModal(VisualTreeAsset contentTemplate, VisualTreeAsset fundsInvestProposalSubmitModalTemplate, VisualTreeAsset investProposalVoteModalTemplate) : base(contentTemplate) {
        this.fundsInvestProposalSubmitModalTemplate = fundsInvestProposalSubmitModalTemplate;
        this.investProposalVoteModalTemplate = investProposalVoteModalTemplate;
    }

    protected override void InitializeContent() {
        investCards = this.modalContent.Query<Button>(className: "invest-card__wrapper").ToList();
        foreach (var investCard in investCards) {
            if (investCard != null) {
                investCard.clicked += () => {
                    this.OnInvestCardButtonClicked(investCard);
                };
            }
        }
    }

    private void OnInvestCardButtonClicked(VisualElement investCard) {
        ModalManager.Instance.ShowModal(new FundsInvestProposalSubmitModal(this.fundsInvestProposalSubmitModalTemplate, this.investProposalVoteModalTemplate));
    }

    protected override void OnClose() {
        foreach (var investCard in investCards) {
            if (investCard != null) {
                investCard.clicked -= () => {
                    this.OnInvestCardButtonClicked(investCard);
                };
            }
        }
    }
}