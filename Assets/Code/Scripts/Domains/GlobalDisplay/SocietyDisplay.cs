public class SocietyDisplay : GlobalDisplay {
    public SocietyDisplay(int currentValue) {
        this.CurrentValue = currentValue;
        this.latestValues.Add(currentValue);
        this.CurrentTrend = DisplayTrend.Trend.stagnant;
    }
}