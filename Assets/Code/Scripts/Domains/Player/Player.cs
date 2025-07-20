using Mirror;

public class Player : NetworkBehaviour {

    protected int id;
    protected string displayName;
    protected HealthDisplay healthDisplay;
    protected MoneyDisplay moneyDisplay;

    public int Id {
        get;
    }

    public string DisplayName {
        get;
    }

    public Player(int id, string displayName) {
        this.Id = id;
        this.DisplayName = displayName;
    }

    public (int, DisplayTrend) GetHealth() {
        return (this.healthDisplay.CurrentValue, this.healthDisplay.CurrentTrend);
    }

    public void AddHealth(int value) {
        this.healthDisplay.AddCurrentValue(value);
    }

    public void SubtractHealth(int value) {
        this.healthDisplay.SubtractCurrentValue(value);
    }

    public (int, DisplayTrend) GetMoney() {
        return (this.moneyDisplay.CurrentValue, this.moneyDisplay.CurrentTrend);
    }

    public void AddMoney(int value, FundsDisplay fundsDisplay) {
        this.moneyDisplay.AddCurrentValue(value, fundsDisplay);
    }

    public void SubtractMoney(int value) {
        this.moneyDisplay.SubtractCurrentValue(value);
    }
}