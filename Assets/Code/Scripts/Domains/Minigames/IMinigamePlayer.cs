using Mirror;

public interface IMinigameRewardHandler {
    /// <summary>
    /// Called on server when minigame rewards should be applied to this player upon exiting
    /// </summary>
    [Server]
    void HandleMinigameRewards(BoardPlayer player);
}