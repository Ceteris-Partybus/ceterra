public class EconomyDisplay : GlobalDisplay {
    public EconomyDisplay(int currentValue) {
        this.CurrentValue = currentValue;
        this.latestValues.Add(currentValue);
        this.CurrentTrend = DisplayTrend.Trend.stagnant;
    }
}