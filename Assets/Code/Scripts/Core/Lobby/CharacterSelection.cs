using UnityEngine;

public class CharacterSelection : MonoBehaviour {
    [SerializeField] private GameObject[] selectablePrefabs;

    public GameObject[] SelectablePrefabs => selectablePrefabs;
}