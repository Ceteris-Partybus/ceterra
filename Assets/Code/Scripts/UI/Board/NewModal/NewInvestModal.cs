using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class NewInvestModal : NewModal {

    private List<VisualElement> investCards = new();

    protected override void Start() {
        this.visualTreeAsset = ModalMap.Instance.InvestModalTemplate;
        base.Start();
    }

    protected override void OnModalShown() {
        investCards = modalElement.Query<VisualElement>(name: "investments-grid").Children<VisualElement>().ToList();

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
        NewModalManager.Instance.Show(FindObjectsByType<NewInvestDepositModal>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).FirstOrDefault());
        // ModalManager.Instance.ShowModal(new InvestDepositModal(this.investDepositModal, investmentIdentifier));
    }

    private void OnInvestCardProposeInvestButtonClicked(VisualElement investCard) {
        // Handle propose invest button click
        Debug.Log("Propose invest button clicked for invest card: " + investCard.name);
        NewModalManager.Instance.Show(FindObjectsByType<NewFundsInvestProposalSubmitModal>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).FirstOrDefault());

    }
}