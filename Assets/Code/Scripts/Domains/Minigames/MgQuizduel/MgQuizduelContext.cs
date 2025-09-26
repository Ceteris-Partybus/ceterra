using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Mirror;
using UnityEngine;

public class MgQuizduelContext : NetworkedSingleton<MgQuizduelContext> {
    [SerializeField] private float GAME_DURATION;
    [SerializeField] private float SCOREBOARD_DURATION;
    [SerializeField] private int MAX_QUESTIONS;
    [SerializeField] private string QUESTION_FILE_NAME;

    public int MaxQuestions => MAX_QUESTIONS;
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
        countdownTimer = GAME_DURATION;
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

        while (timeElapsed < GAME_DURATION && isQuizActive) {
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
        yield return new WaitForSeconds(SCOREBOARD_DURATION);

        StopQuiz();
        GameManager.Singleton.EndMinigame();
    }

    private void StartQuizduel() {
        MgQuizduelController.Instance.UpdateQuizUI(quizduelQuestions[MgQuizduelController.Instance.CurrentQuestionIndex]);
    }

    private void InitializeQuiz() {
        var jsonFile = Resources.Load<TextAsset>(QUESTION_FILE_NAME);
        quizduelQuestions = JsonConvert.DeserializeObject<List<QuestionData>>(jsonFile.text)
            .OrderBy(x => UnityEngine.Random.Range(0f, 1f))
            .Take(MAX_QUESTIONS)
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