using UnityEngine;

public class GameState : MonoBehaviour {
    private Board board;

    void Start() {
        board = new Board();
    }

    public Board Board {
        get {
            return board;
        }
    }
}