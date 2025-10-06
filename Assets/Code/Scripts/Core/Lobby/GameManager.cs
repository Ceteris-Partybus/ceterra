using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : NetworkRoomManager {
    public static GameManager Singleton {
        get {
            return NetworkManager.singleton as GameManager;
        }
    }

    [Header("Scenes")]
    [SerializeField][Scene] private string endScene;

    [Header("Character Selection")]
    [SerializeField] private GameObject[] selectableCharacters;
    public int CharacterCount => selectableCharacters.Length;
    public GameObject GetCharacter(int index) => selectableCharacters[index];
    [SerializeField] private GameObject[] selectableDices;
    public int DiceCount => selectableDices.Length;
    public GameObject GetDice(int index) => selectableDices[index];

    [Header("Minigames")]
    [SerializeField]
    private List<string> minigameScenes = new();
    public List<string> MinigameScenes => minigameScenes;

    [SerializeField]
    private List<string> playedMinigames = new();
    public List<string> PlayedMinigames => playedMinigames;

    [Header("Round management")]
    [SerializeField]
    private int maxRounds = 1;
    public int MaxRounds => maxRounds;

    [SerializeField]
    private int currentRound = 1;
    public int CurrentRound => currentRound;
    [SerializeField]
    private int playersPassedStart = 0;

    public int[] PlayerIds => roomSlots.Select(slot => slot.index).ToArray();

    public void IncrementPlayersPassedStart() {
        playersPassedStart++;
        if (playersPassedStart >= roomSlots.Count) {
            currentRound++;
            playersPassedStart = 0;
        }

        if (currentRound > maxRounds) {
            StartCoroutine(EndGameAfterPlayerMoveFinish());
        }
    }

    private IEnumerator EndGameAfterPlayerMoveFinish() {
        yield return new WaitUntil(() => BoardContext.Instance?.IsAnyPlayerMoving() == false
        && BoardContext.Instance?.IsAnyPlayerInAnimation() == false
        && BoardContext.Instance?.IsAnyPlayerChoosingJunction() == false
        && BoardContext.Instance?.CurrentState == BoardContext.State.PLAYER_TURN);
        ServerChangeScene(endScene);
    }

    public override void OnRoomServerSceneChanged(string sceneName) {
        Debug.Log($"[Server] Scene changed to {sceneName}");

        foreach (var player in FindObjectsByType<SceneConditionalPlayer>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
            player.HandleSceneChange(sceneName);
        }

        if (sceneName == GameplayScene) {
            BoardContext.Instance?.StartPlayerTurn();
        }
    }

    public override void OnClientSceneChanged() {
        base.OnClientSceneChanged();
        if (networkSceneName == GameplayScene) {
            foreach (var field in FindObjectsByType<FieldBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
                field.Show();
            }
        }
        else {
            foreach (var field in FindObjectsByType<FieldBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
                field.Hide();
            }
        }
    }

    public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnectionToClient conn, GameObject roomPlayer, GameObject gamePlayer) {
        var lobbyPlayer = roomPlayer.GetComponent<LobbyPlayer>();

        foreach (var scenePlayer in gamePlayer.GetComponents<SceneConditionalPlayer>()) {
            scenePlayer.SetPlayerData(lobbyPlayer.index, lobbyPlayer.PlayerName);
        }

        gamePlayer.GetComponent<BoardPlayer>().ServerTransferCharacterSelection(lobbyPlayer);
        return base.OnRoomServerSceneLoadedForPlayer(conn, roomPlayer, gamePlayer);
    }

    /// <summary>
    /// Called when the server wants to start a minigame.
    /// This method will change the scene to the specified minigame scene.
    /// </summary>
    /// <param name="sceneName">The name of the minigame scene to start.</param>
    private void StartMinigame(string sceneName) {
        if (!MinigameScenes.Contains(sceneName)) {
            Debug.LogError($"Scene {sceneName} is not a valid minigame scene.");
            return;
        }

        if (playedMinigames.Count + 1 == MinigameScenes.Count) {
            playedMinigames.Clear();
        }
        else {
            if (playedMinigames.Contains(sceneName)) {
                Debug.LogError($"Scene {sceneName} has already been played in the current rotation. It should not be played again until all other minigames have been played.");
                return;
            }
        }

        playedMinigames.Add(sceneName);

        if (NetworkServer.active) {
            ServerChangeScene(sceneName);
        }
    }

    public void StartMinigame() {
        var availableMinigames = MinigameScenes.Except(playedMinigames).ToList();
        if (availableMinigames.Count == 0) {
            Debug.LogError("No available minigames to start.");
            return;
        }

        var randomIndex = UnityEngine.Random.Range(0, availableMinigames.Count);
        var selectedMinigame = availableMinigames[randomIndex];
        StartMinigame(selectedMinigame);
    }

    /// <summary>
    /// Called when the minigame ends and the game should return to the main gameplay scene.
    /// </summary>
    public void EndMinigame() {
        if (NetworkServer.active) {
            ServerChangeScene(GameplayScene);
        }
    }
}