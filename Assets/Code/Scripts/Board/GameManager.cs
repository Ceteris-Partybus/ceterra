using UnityEngine;
using Mirror;
using System;

public class GameManager : NetworkedSingleton<GameManager> {

    [Header("Game Settings")]
    [SerializeField]
    private readonly int minPlayersToStart = 2;
    public int MinPlayersToStart => minPlayersToStart;

    [SyncVar(hook = nameof(OnStateChanged))]
    private State state = State.WAITING_FOR_PLAYERS;
    public State CurrentState => state;

    [SyncVar]
    public int connectedPlayers = 0;

    private readonly SyncList<Player> players = new SyncList<Player>();

    // Public property to allow BoardContext to access players
    public SyncList<Player> Players => players;

    // Public property to access BoardContext's FieldList
    public FieldList FieldList => BoardContext.Instance?.FieldList;

    public enum State {
        WAITING_FOR_PLAYERS,
        ON_BOARD,
        IN_MINIGAME,
        GAME_ENDED,
    }

    [Server]
    public void RegisterPlayer(Player player) {

        if (!players.Contains(player)) {
            players.Add(player);
            connectedPlayers = players.Count;

            if (FieldList?.Head != null) {
                var startPosition = FieldList.Head.Position;
                startPosition.y += 1f;
                player.transform.position = startPosition;
                player.currentSplineKnotIndex = FieldList.Head.SplineKnotIndex;
            }

            if (connectedPlayers >= minPlayersToStart && state == State.WAITING_FOR_PLAYERS) {
                StartGame();
            }
        }
    }

    [Server]
    public void UnregisterPlayer(Player player) {

        if (players.Remove(player)) {
            connectedPlayers = players.Count;

            if (connectedPlayers < minPlayersToStart) {
                state = State.WAITING_FOR_PLAYERS;
            }
        }
    }

    [Server]
    void StartGame() {
        state = State.ON_BOARD;
        RpcGameStarted();

        Invoke(nameof(DelayedStartPlayerTurn), 1f); // Wenn GameManager die Methode hier nur in einer anderen Szene als der 
        // Board Szene aufruft, kann StartPlayerTurn auch einfach in Start von BoardContext aufgerufen werden.
    }

    [Server]
    void DelayedStartPlayerTurn() {
        BoardContext.Instance?.StartPlayerTurn();
    }

    [ClientRpc]
    void RpcGameStarted() {

    }

    public void OnStateChanged(State oldState, State newState) {
    }
}