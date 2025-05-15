using System;

public class ResourceDisplay : GlobalDisplay {
    public ResourceDisplay(int currentValue) {
        this.CurrentValue = currentValue;
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