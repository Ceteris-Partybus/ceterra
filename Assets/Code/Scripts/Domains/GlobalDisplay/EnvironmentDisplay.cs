public class EnvironmentDisplay : GlobalDisplay {
    public EnvironmentDisplay(int currentValue) {
        this.CurrentValue = currentValue;
        this.latestValues.Add(currentValue);
        this.CurrentTrend = DisplayTrend.Trend.stagnant;
    }
}