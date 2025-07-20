using System;

public class MoneyDisplay : GeneralDisplay {
    public MoneyDisplay(int currentValue) : base(currentValue) { }

    public void AddCurrentValue(int add, FundsDisplay fundsDisplay) {
        if (this.CurrentValue + add > this.MaxValue) {
            int diff = this.CurrentValue + add - this.MaxValue;
            fundsDisplay.AddCurrentValue(diff);
            this.CurrentValue = this.MaxValue;
        }
        else {
            this.CurrentValue += add;
        }
    }

    public override void SubtractCurrentValue(int subtract) {
        if (this.CurrentValue - subtract < this.MinValue) {
            throw new Exception("Subtraction exceeds minimum value " + this.MinValue);
        }
        else {
            this.CurrentValue -= subtract;
        }
    }
}