using UnityEngine;
using UnityEngine.UIElements;

public class ModalMap : NetworkedSingleton<ModalMap> {
    [SerializeField] public VisualTreeAsset ResourceModalTemplate;
    [SerializeField] public VisualTreeAsset FundsModalTemplate;
    [SerializeField] public VisualTreeAsset InvestModalTemplate;
}