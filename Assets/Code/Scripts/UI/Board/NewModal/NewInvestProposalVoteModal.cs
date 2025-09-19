public class NewInvestProposalVoteModal : NewModal {

    protected override void Start() {
        this.visualTreeAsset = ModalMap.Instance.InvestProposalVoteModalTemplate;
        base.Start();
    }

    protected override void OnModalShown() {
        // Setup your specific modal logic here
    }
}