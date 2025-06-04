using System;

public class FundsDisplay : GlobalDisplay {
    public FundsDisplay(int currentValue) : base(currentValue) {
        this.MaxValue = Int32.MaxValue;
    }

    public override void SubtractCurrentValue(int subtract) {
        if (this.CurrentValue - subtract < this.MinValue) {
            throw new Exception("Subtraction exceeds minimum value " + this.MinValue);
        }
        else {
            this.CurrentValue -= subtract;
        }
    }

    public override void AddCurrentValue(int add) {
        if (this.MaxValue - this.CurrentValue < add) {
            this.CurrentValue = this.MaxValue;
        }
        else {
            this.CurrentValue += add;
        }
    }
}