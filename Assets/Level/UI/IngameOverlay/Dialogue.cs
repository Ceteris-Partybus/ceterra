using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "Dialogue", menuName = "Scriptable Objects/Dialogue")]
public class Dialogue : ScriptableObject {
    public string title;
    public VectorImage icon;
    public VisualTreeAsset content;
}
