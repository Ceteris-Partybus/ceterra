using UnityEngine;
using UnityEngine.Splines;

public class EventField : Field {
    public EventField(int id, int splineId, SplineKnotIndex splineKnotIndex, Vector3 position)
        : base(id, splineId, FieldType.EVENT, splineKnotIndex, position) {
    }

    public override void Invoke(BoardPlayer player) {
        Debug.Log($"Player {player.PlayerName} landed on an event field.");

        MinigameManager mm = MinigameManager.Instance;
        if (mm != null) {
            string[] availableMinigames = mm.GetAllMinigameScenes();
            if (availableMinigames != null && availableMinigames.Length > 0) {
                // For now, start the first minigame. You can implement random selection or specific logic here
                string selectedMinigame = availableMinigames[0];
                Debug.Log($"Starting minigame: {selectedMinigame}");
                mm.StartMinigame(selectedMinigame);
            }
            else {
                Debug.LogWarning("No minigames available!");
            }
        }
        else {
            Debug.LogError("MinigameManager not found!");
        }
    }
}