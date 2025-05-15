using System;

public class FondsDisplay : GlobalDisplay {
    public FondsDisplay(int currentValue) {
        this.CurrentValue = currentValue;
        this.MaxValue = 2147483647;
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