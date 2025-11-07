using Mirror;
using System;
using UnityEngine;

[Serializable]
public class PlayerStats : NetworkBehaviour {
    #region Player

    [SerializeField]
    private BoardPlayer player;

    #endregion

    #region Serialized Fields

    [SerializeField]
    [SyncVar(hook = nameof(OnHealthChanged))]
    private int health;

    [SerializeField]
    [SyncVar(hook = nameof(OnCoinsChanged))]
    private int coins;

    [SerializeField]
    [SyncVar(hook = nameof(OnScoreChanged))]
    private int score;

    #endregion

    #region Events

    public event Action<int> OnHealthUpdated;
    public event Action<int> OnCoinsUpdated;
    public event Action<int> OnScoreUpdated;

    #endregion

    #region Unity Lifecycle

    [ServerCallback]
    public override void OnStartServer() {
        base.OnStartServer();
        InitializeStats();
    }

    #endregion

    #region Initialization

    private void InitializeStats() {
        health = Constants.STARTING_HEALTH;
        coins = Constants.STARTING_COINS;
        score = Constants.STARTING_SCORE;
    }

    #endregion

    #region Health Management

    [Server]

    public void SetHealth(int newHealth) {
        health = Mathf.Clamp(newHealth, Constants.MIN_HEALTH, Constants.MAX_HEALTH);
    }

    [Server]
    public void ModifyHealth(int amount) {
        SetHealth(health + amount);
    }

    public int GetHealth() {
        return health;
    }

    private void OnHealthChanged(int oldValue, int newValue) {
        OnHealthUpdated?.Invoke(newValue);
    }

    #endregion

    #region Coins Management

    [Server]
    public void SetCoins(int newCoins) {
        coins = Mathf.Max(0, newCoins);
    }

    [Server]
    public void ModifyCoins(int amount) {
        SetCoins(coins + amount);
    }

    public int GetCoins() {
        return coins;
    }

    private void OnCoinsChanged(int oldValue, int newValue) {
        OnCoinsUpdated?.Invoke(newValue);
    }

    #endregion

    #region Score Management

    [Server]
    public void SetScore(int newScore) {
        score = Mathf.Max(0, newScore);
    }

    [Server]
    public void ModifyScore(int amount) {
        SetScore(score + amount);
    }

    public int GetScore() {
        return score;
    }

    private void OnScoreChanged(int oldValue, int newValue) {
        OnScoreUpdated?.Invoke(newValue);
    }

    #endregion
}