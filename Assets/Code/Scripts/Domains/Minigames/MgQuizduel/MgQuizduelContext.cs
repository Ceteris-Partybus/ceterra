using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Mirror;
using UnityEngine;

public class MgQuizduelContext : NetworkedSingleton<MgQuizduelContext> {

    [Header("Minigame Settings")]
    [SerializeField] private float GameDuration;
    [SerializeField] private float scoreboardDuration;
    [SerializeField] private int maxQuestions;
    [SerializeField] private string questionFileName;

    public int MaxQuestions => maxQuestions;
    private float countdownTimer;
    private List<QuestionData> quizduelQuestions = new();
    private bool isQuizActive = false;
    private Coroutine countdownCoroutine;

    protected override void Start() {
        base.Start();
        InitializeQuiz();
        if (isServer) {
            StartCoroutine(QuizduelRoutine());
            countdownCoroutine = StartCoroutine(UpdateCountdown());
        }
    }

    [Server]
    private IEnumerator UpdateCountdown() {
        countdownTimer = GameDuration;
        var lastSeconds = Mathf.CeilToInt(countdownTimer);

        RpcUpdateCountdown(lastSeconds);

        while (countdownTimer > 0f) {
            countdownTimer -= Time.deltaTime;
            var seconds = Mathf.CeilToInt(Mathf.Max(0f, countdownTimer));
            if (seconds != lastSeconds) {
                RpcUpdateCountdown(seconds);
                lastSeconds = seconds;
            }
            yield return null;
        }
        RpcUpdateCountdown(0);
        countdownCoroutine = null;
    }

    [ClientRpc]
    private void RpcUpdateCountdown(int seconds) {
        MgQuizduelController.Instance.UpdateCountdown(seconds);
    }

    private IEnumerator QuizduelRoutine() {
        isQuizActive = true;
        var timeElapsed = 0f;

        yield return new WaitForSeconds(0.5f);
        StartQuizduel();

        while (timeElapsed < GameDuration && isQuizActive) {
            if (AreAllPlayersFinished()) {
                break;
            }
            yield return new WaitForSeconds(1f);
            timeElapsed += 1f;
        }

        var allActivePlayers = FindObjectsByType<MgQuizduelPlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
            .Where(p => p.IsActiveForCurrentScene)
            .OrderByDescending(p => p.Score)
            .ToList();

        var rankings = new List<MgQuizduelPlayerRankingData>();
        for (var i = 0; i < allActivePlayers.Count; i++) {
            var player = allActivePlayers[i];
            var rank = i + 1;
            var reward = CalculateCoinReward(rank);

            player.SetEarnedCoinReward(reward);

            rankings.Add(MgQuizduelPlayerRankingData.FromPlayer(player, rank));
        }
        isQuizActive = false;

        MgQuizduelController.Instance.ShowScoreboard(rankings);
        yield return new WaitForSeconds(scoreboardDuration);

        StopQuiz();
        GameManager.Singleton.EndMinigame();
    }

    private void StartQuizduel() {
        MgQuizduelController.Instance.UpdateQuizUI(quizduelQuestions[MgQuizduelController.Instance.CurrentQuestionIndex]);
    }

    private void InitializeQuiz() {
        var jsonFile = Resources.Load<TextAsset>(questionFileName);
        quizduelQuestions = JsonConvert.DeserializeObject<List<QuestionData>>(jsonFile.text)
            .OrderBy(x => UnityEngine.Random.Range(0f, 1f))
            .Take(maxQuestions)
            .ToList();
    }

    public MgQuizduelPlayer GetLocalPlayer() {
        return FindObjectsByType<MgQuizduelPlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
            .FirstOrDefault(p => p.isLocalPlayer);
    }

    public bool ProcessPlayerAnswer(int answerIndex) {
        var localPlayer = GetLocalPlayer();
        var currentQuestionIndex = MgQuizduelController.Instance.CurrentQuestionIndex;
        var isCorrect = quizduelQuestions[currentQuestionIndex].isCorrectAnswer(answerIndex);

        if (isCorrect) {
            localPlayer.CmdAddScore(1);
        }
        if (currentQuestionIndex + 1 >= MaxQuestions) {
            localPlayer.CmdSetFinishedQuiz();
        }

        return isCorrect;
    }
    private bool AreAllPlayersFinished() {
        var allActivePlayers = FindObjectsByType<MgQuizduelPlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
            .Where(p => p.IsActiveForCurrentScene)
            .ToList();

        if (allActivePlayers.Count == 0) {
            return false;
        }

        var allFinished = allActivePlayers.All(player => player.HasFinishedQuiz);

        return allFinished;
    }

    private int CalculateCoinReward(int rank) {
        return 100 / (int)Mathf.Pow(2, rank - 1);
    }

    internal QuestionData GetQuestion(int currentQuestionIndex) {
        return quizduelQuestions[currentQuestionIndex];
    }

    [Server]
    public void StopQuiz() {
        if (countdownCoroutine != null) {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }
    }
}