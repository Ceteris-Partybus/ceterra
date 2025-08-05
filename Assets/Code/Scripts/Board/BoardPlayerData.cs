using UnityEngine.Splines;

public class BoardPlayerData {
    public BoardPlayerData() {
        healthDisplay = new HealthDisplay(100);
        moneyDisplay = new MoneyDisplay(0);
        SplineKnotIndex = BoardContext.Instance.FieldList.Head.SplineKnotIndex;
    }

    private readonly HealthDisplay healthDisplay;
    private readonly MoneyDisplay moneyDisplay;
    private SplineKnotIndex splineKnotIndex;

    public HealthDisplay HealthDisplay => healthDisplay;

    public MoneyDisplay MoneyDisplay => moneyDisplay;

    public SplineKnotIndex SplineKnotIndex { get => splineKnotIndex; set => splineKnotIndex = value; }
}