using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;
using System;
using System.Linq;

public class CurrentTurnManager : NetworkedSingleton<CurrentTurnManager> {
    [SerializeField] private UIDocument uiDocument;
    private VisualElement rootElement;
    private VisualElement turnInfoPanel;
    private Label currentRoundLabel;
    private Label currentPlayerNameLabel;
    private Button rollDiceButton;
    private Button boardButton;
    private Button settingsButton;
    private BoardPlayer boardPlayer;
    private VisualElement announcementOverlay;
    private Label announcementText;
    private Sequence currentAnnouncementSequence;
    private int lastAnnouncedRound = 0;

    protected override void Start() {
        rootElement = uiDocument.rootVisualElement;
        currentRoundLabel = rootElement.Q<Label>("current-round-text");
        currentPlayerNameLabel = rootElement.Q<Label>("current-turn-text");
        rollDiceButton = rootElement.Q<Button>("roll-dice-button");
        rollDiceButton.clicked += OnRollDiceButtonClicked;

        boardButton = rootElement.Q<Button>("board-button");
        boardButton.clicked += OnBoardButtonClicked;

        settingsButton = rootElement.Q<Button>("settings-button");
        settingsButton.clicked += SettingsController.Instance.OpenSettingsPanel;

        announcementOverlay = rootElement.Q<VisualElement>("announcement-overlay");
        announcementText = rootElement.Q<Label>("announcement-text");
        turnInfoPanel = rootElement.Q<VisualElement>("turn-info-panel");

        BoardContext.Instance.OnNextPlayerTurn += UpdateTurnUI;
        boardPlayer = BoardContext.Instance.GetLocalPlayer();
        boardPlayer.OnDiceRollEnded += HideRollDiceButton;
        base.Start();
    }

    private void UpdateTurnUI(BoardPlayer currentPlayer, int currentRound, int maxRounds) {
        currentRoundLabel.text = $"{currentRound} / {maxRounds}";
        currentPlayerNameLabel.text = currentPlayer.PlayerName;

        HideUIElements();
        currentAnnouncementSequence?.Kill();
        announcementOverlay.style.display = DisplayStyle.Flex;
        if (currentRound > lastAnnouncedRound) {
            lastAnnouncedRound = currentRound;
            currentAnnouncementSequence = ShowSequentialAnnouncements(currentRound, currentPlayer.PlayerName);
        }
        else {
            currentAnnouncementSequence = ShowAnnouncementText(() => FormatTurnMessage(currentPlayer.PlayerName));
        }

        currentAnnouncementSequence.OnComplete(() => {
            announcementOverlay.style.display = DisplayStyle.None;
            ShowUIElementsAnimated(currentPlayer.isLocalPlayer);
        });
    }

    private string FormatTurnMessage(string playerName) {
        var possessive = playerName.EndsWith("s") ? "'" : "'s";
        return $"It's {playerName}{possessive} turn";
    }

    private void OnRollDiceButtonClicked() {
        Audiomanager.Instance?.PlayClickSound();
        if (BoardContext.Instance.CurrentState != BoardContext.State.PLAYER_TURN) { return; }

        SetButtonsInteractable(false);

        boardPlayer.CmdToggleDiceRoll();
        rollDiceButton.text = rollDiceButton.text == "Roll Dice" ? "Cancel Roll" : "Roll Dice";

        var animationSequence = DOTween.Sequence();

        if (IsButtonVisible(boardButton)) {
            animationSequence.Join(UIAnimationUtils.SlideOutToLeft(boardButton));
        }
        else {
            animationSequence.Join(UIAnimationUtils.SlideInFromLeft(boardButton, UIAnimationUtils.BUTTON_FINAL_POSITION, .4f));
        }

        animationSequence.OnComplete(() => SetButtonsInteractable(true));
    }

    private void OnBoardButtonClicked() {
        Audiomanager.Instance?.PlayClickSound();
        if (BoardContext.Instance.CurrentState != BoardContext.State.PLAYER_TURN) { return; }

        SetButtonsInteractable(false);

        boardPlayer.CmdToggleBoardOverview();
        boardButton.text = boardButton.text == "View Board" ? "Go back to Player" : "View Board";

        var animationSequence = DOTween.Sequence();

        if (IsButtonVisible(rollDiceButton)) {
            animationSequence.Join(UIAnimationUtils.SlideOutToLeft(rollDiceButton));
        }
        else {
            animationSequence.Join(UIAnimationUtils.SlideInFromLeft(rollDiceButton, UIAnimationUtils.BUTTON_FINAL_POSITION, .4f));
        }

        animationSequence.OnComplete(() => SetButtonsInteractable(true));
    }

    private void HideUIElements() {
        foreach (var element in new[] { rollDiceButton, boardButton, settingsButton, turnInfoPanel }) {
            element.style.display = DisplayStyle.None;
            element.style.left = UIAnimationUtils.SLIDE_OUT_POSITION;
        }
    }

    private void HideRollDiceButton() {
        DOTween.Sequence()
            .AppendCallback(() => SetButtonsInteractable(false))
            .Join(UIAnimationUtils.SlideOutToLeft(rollDiceButton))
            .OnComplete(() => SetButtonsInteractable(true));
    }

    private void ShowUIElementsAnimated(bool isLocalPlayer) {
        SetButtonsInteractable(false);

        var sequence = DOTween.Sequence()
            .Join(UIAnimationUtils.SlideInFromLeft(turnInfoPanel, UIAnimationUtils.INFO_PANEL_FINAL_POSITION))
            .Join(UIAnimationUtils.SlideInFromLeft(settingsButton, UIAnimationUtils.BUTTON_FINAL_POSITION));

        if (isLocalPlayer) {
            sequence
                .Join(UIAnimationUtils.SlideInFromLeft(rollDiceButton, UIAnimationUtils.BUTTON_FINAL_POSITION))
                .Join(UIAnimationUtils.SlideInFromLeft(boardButton, UIAnimationUtils.BUTTON_FINAL_POSITION));
        }

        sequence.OnComplete(() => SetButtonsInteractable(true));
    }

    private Sequence ShowSequentialAnnouncements(int roundNumber, string playerName) {
        return ShowAnnouncementText(() => $"ROUND {roundNumber}")
            .Append(ShowAnnouncementText(() => FormatTurnMessage(playerName)));
    }

    private Sequence ShowAnnouncementText(Func<string> getMessage) {
        return DOTween.Sequence()
            .AppendCallback(() => announcementText.text = getMessage())
            .Append(UIAnimationUtils.ScaleAndFadeIn(announcementText, duration: .5f))
            .AppendInterval(1f)
            .Append(UIAnimationUtils.AnimateScale(announcementText, 1f, .8f, .3f, Ease.InCubic))
            .Join(UIAnimationUtils.AnimateFade(announcementText, 1f, 0f, .3f, Ease.InCubic));
    }

    private bool IsButtonVisible(Button button) {
        return button.style.display == DisplayStyle.Flex;
    }

    private void SetButtonsInteractable(bool interactable) {
        rollDiceButton.SetEnabled(interactable);
        boardButton.SetEnabled(interactable);
        settingsButton.SetEnabled(interactable);
    }

    private void OnDestroy() {
        currentAnnouncementSequence?.Kill();

        if (BoardContext.Instance != null) {
            BoardContext.Instance.OnNextPlayerTurn -= UpdateTurnUI;
        }

        if (boardPlayer != null) {
            boardPlayer.OnDiceRollEnded -= HideRollDiceButton;
        }
    }
}