public class Player {

    protected int id;
    protected string name;
    protected HealthDisplay healthDisplay;
    protected MoneyDisplay moneyDisplay;

    public int Id {
        get;
    }

    public string Name {
        get;
    }

    public Player(int id, string name) {
        this.Id = id;
        this.Name = name;
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

    public void AddMoney(int value) {
        this.moneyDisplay.AddCurrentValue(value);
    }

    public void SubtractMoney(int value) {
        this.moneyDisplay.SubtractCurrentValue(value);
    }
}