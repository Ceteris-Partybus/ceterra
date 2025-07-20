using UnityEngine;

public class BoardField : MonoBehaviour {
    [Header("Field Settings")]
    public int fieldNumber;
    public Color fieldColor = Color.white;
    public bool isSpecialField = false;

    void Start() {
        gameObject.tag = "BoardField";

        if (fieldNumber == 0) {
            if (int.TryParse(name.Replace("Field", ""), out int number)) {
                fieldNumber = number;
            }
        }
    }

    void OnTriggerEnter(Collider other) {
        BoardPlayer player = other.GetComponent<BoardPlayer>();
        if (player != null) {
            OnPlayerEnterField(player);
        }
    }

    void OnPlayerEnterField(BoardPlayer player) {
        Debug.Log($"{player.playerName} entered field {fieldNumber}");

        if (isSpecialField) {
            HandleSpecialField(player);
        }
    }

    void HandleSpecialField(BoardPlayer player) {
        Debug.Log($"Special field effect for {player.playerName}!");
    }
}