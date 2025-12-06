using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class InvestProposalVoteModal : Modal {

    public enum VoteResult {
        PENDING,
        APPROVED,
        REJECTED
    }

    public static InvestProposalVoteModal Instance => GetInstance<InvestProposalVoteModal>();
    protected override string GetHeaderTitle() {
        return LocalizationManager.Instance.GetLocalizedText(56156036074037248);
    }
    private bool hasVoted;
    private Dictionary<int, VoteResult> votes = new();

    private Label requiredFundsLabel;
    private Label requiredResourcesLabel;
    private Label cooldownLabel;
    private Button voteYesButton;
    private Button voteNoButton;
    private VisualElement votings;
    private Label infoTextLabel;

    public int InvestmentId;
    private Investment VoteInvestment => BoardContext.Instance.investments.Find(inv => inv.id == InvestmentId);
    public int PlayerId;
    private BoardPlayer player => BoardContext.Instance.GetPlayerById(PlayerId);
    public int ProposedCoins;

    protected override void Start() {
        this.closeOnBackgroundClick = false;
        this.closeOnEscapeKey = false;
        this.showCloseButton = false;
        this.visualTreeAsset = ModalMap.Instance.InvestProposalVoteModalTemplate;
        showModalTypeInHeader = true;
        base.Start();
    }

    protected override void OnModalShown() {
        requiredFundsLabel = modalElement.Q<Label>("required-funds");
        requiredResourcesLabel = modalElement.Q<Label>("required-resources");
        cooldownLabel = modalElement.Q<Label>("cooldown");
        voteYesButton = modalElement.Q<Button>("vote-yes");
        voteNoButton = modalElement.Q<Button>("vote-no");
        votings = modalElement.Q<VisualElement>("votings");
        infoTextLabel = modalElement.Q<Label>("info-text");

        infoTextLabel.text = LocalizationManager.Instance.GetLocalizedText(56646535894892544, new object[] { player.PlayerName, ProposedCoins, LocalizationManager.Instance.GetLocalizedText(VoteInvestment.displayName) });

        requiredFundsLabel.text = (VoteInvestment.requiredMoney - VoteInvestment.currentMoney).ToString();
        requiredResourcesLabel.text = VoteInvestment.requiredResources.ToString();
        cooldownLabel.text = VoteInvestment.cooldown.ToString();

        this.hasVoted = false;

        this.votes.Clear();
        votings.Clear();

        foreach (int playerId in GameManager.Singleton.PlayerIds) {
            bool isProposer = playerId == PlayerId;

            votes[playerId] = isProposer ? VoteResult.APPROVED : VoteResult.PENDING;

            var voteEntry = new VisualElement();
            voteEntry.AddToClassList("vote-entry");
            voteEntry.AddToClassList(isProposer ? "approved" : "pending");

            votings.Add(voteEntry);
        }

        int localPlayerId = BoardContext.Instance.GetLocalPlayer().PlayerId;

        if (PlayerId == localPlayerId) {
            voteYesButton.SetEnabled(false);
            voteNoButton.SetEnabled(false);
        }
        else {
            voteYesButton.clicked += () => OnVoteButtonClicked(true);
            voteNoButton.clicked += () => OnVoteButtonClicked(false);
        }

        if (BoardContext.Instance.GetAllPlayers().Count == 1) {
            // Auto-approve if only one player is in the game
            UpdateVotings(localPlayerId, true);
        }
    }

    [ClientCallback]
    private void Update() {
        if (!IsVisible()) {
            return;
        }

        foreach (var voteEntry in votings.Children()) {
            if (voteEntry.ClassListContains("pending")) {
                // TODO use dotween instead :)
                var newRotation = Quaternion.AngleAxis(Time.time * 360f, Vector3.forward);
                voteEntry.transform.rotation = newRotation;
            }
        }
    }

    [ClientCallback]
    private void OnVoteButtonClicked(bool agreed) {
        Audiomanager.Instance?.PlayClickSound();
        if (hasVoted) {
            return;
        }

        int localPlayerId = BoardContext.Instance.GetLocalPlayer().PlayerId;

        CmdSendVote(localPlayerId, agreed);

        hasVoted = true;
        voteYesButton.SetEnabled(false);
        voteNoButton.SetEnabled(false);
    }

    [ClientCallback]
    public void UpdateVotings(int playerId, bool agreed) {
        if (!votes.ContainsKey(playerId)) {
            Debug.LogWarning($"Player {playerId} not found in votes dictionary");
            return;
        }

        VoteResult newVoteResult = agreed ? VoteResult.APPROVED : VoteResult.REJECTED;
        votes[playerId] = newVoteResult;

        var playerIds = new List<int>(GameManager.Singleton.PlayerIds);
        int playerIndex = playerIds.IndexOf(playerId);

        if (playerIndex < 0 || playerIndex >= votings.childCount) {
            Debug.LogWarning($"Player index {playerIndex} out of range for votings visual elements");
            return;
        }

        var voteEntry = votings[playerIndex];

        voteEntry.RemoveFromClassList("pending");
        voteEntry.RemoveFromClassList("approved");
        voteEntry.RemoveFromClassList("rejected");

        string newClass = agreed ? "approved" : "rejected";
        voteEntry.AddToClassList(newClass);
        voteEntry.transform.rotation = Quaternion.identity;

        CheckVoteOutcome();
    }

    [ClientCallback]
    private void CheckVoteOutcome() {
        if (votes.Values.Contains(VoteResult.PENDING)) {
            return;
        }

        int approveCount = 0;
        int rejectCount = 0;

        foreach (var vote in votes.Values) {
            if (vote == VoteResult.APPROVED) {
                approveCount++;
            }
            else if (vote == VoteResult.REJECTED) {
                rejectCount++;
            }
        }

        bool approved = approveCount >= (votes.Count + 1) / 2;

        int localPlayerId = BoardContext.Instance.GetLocalPlayer().PlayerId;

        if (approved) {
            if (localPlayerId == PlayerId) {
                // Only the proposer needs to send the command to approve the investment
                // Otherwise all modifiers would be applied multiple times
                CmdApproveInvestment(InvestmentId, ProposedCoins);
            }
        }
        else {
            CmdDeclineInvestment();
        }

        ModalManager.Instance.Hide();
    }

    [Command(requiresAuthority = false)]
    private void CmdApproveInvestment(int investmentId, int proposedCoins) {
        BoardContext.Instance.ApproveInvestment(investmentId, proposedCoins);
        RpcUpdateInvestment();
    }

    [ClientRpc]
    private void RpcUpdateInvestment() {
        BoardOverlay.Instance?.RecalculateInvestment(this.InvestmentId);
    }

    [Command(requiresAuthority = false)]
    private void CmdDeclineInvestment() {
        RpcDeclineInvestment();
    }

    [ClientRpc]
    private void RpcDeclineInvestment() {
        InfoModal.Instance.Message = LocalizationManager.Instance.GetLocalizedText(56647493433524224);
        ModalManager.Instance.Show(InfoModal.Instance);
    }

    [Command(requiresAuthority = false)]
    private void CmdSendVote(int playerId, bool agreed) {
        RpcReceiveVote(playerId, agreed);
    }

    [ClientRpc]
    private void RpcReceiveVote(int playerId, bool agreed) {
        UpdateVotings(playerId, agreed);
    }
}