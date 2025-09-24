using UnityEngine;

public class Character : MonoBehaviour {
    [SerializeField] private string characterName;
    [SerializeField] private string info;

    public string CharacterName => characterName;
    public string Info => info;
}