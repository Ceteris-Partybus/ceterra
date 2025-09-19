using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class NewFundsInvestProposalModal : NewModal {

    private List<Button> investCards = new();

    protected override void Start() {
        this.visualTreeAsset = ModalMap.Instance.FundsInvestProposalModalTemplate;
        base.Start();
    }

    protected override void OnModalShown() {
        investCards = modalElement.Query<Button>(className: "invest-card__wrapper").ToList();
        foreach (var investCard in investCards) {
            if (investCard != null) {
                investCard.clicked += () => this.OnInvestCardButtonClicked(investCard);
            }
        }
    }

    private void OnInvestCardButtonClicked(VisualElement investCard) {
        NewModalManager.Instance.Show(FindObjectsByType<NewFundsInvestProposalSubmitModal>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).FirstOrDefault());
    }

    protected override void OnModalHidden() {
        foreach (var investCard in investCards) {
            if (investCard != null) {
                investCard.clicked -= () => {
                    this.OnInvestCardButtonClicked(investCard);
                };
            }
        }
    }
}