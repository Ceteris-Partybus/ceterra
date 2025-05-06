using System;

public class EconomyDisplay {
    private const int minValue = 0;
    private const int maxValue = 100;
    private int currentValue;

    public int MinValue {
        get { return minValue; }
    }

    public int MaxValue {
        get { return maxValue; }
    }

    public int CurrentValue {
        get { return this.currentValue; }
        set {
            if (value < this.MinValue || value > this.MaxValue) {
                throw new Exception("Value must be between " + this.MinValue + " and " + this.MaxValue);
            }
            else {
                this.currentValue = value;
            }
        }
    }

    public EconomyDisplay(int currentValue) {
        this.CurrentValue = currentValue;
    }

    public void AddCurrentValue(int add) {
        if (this.CurrentValue + add > this.MaxValue) {
            this.CurrentValue = this.MaxValue;
        }
        else {
            this.CurrentValue += add;
        }
    }

    public void SubtractCurrentValue(int subtract) {
        if (this.CurrentValue - subtract < this.MinValue) {
            throw new Exception("Subtraction exceeds minimum value " + this.MinValue);
        }
        else {
            this.CurrentValue -= subtract;
        }
    }
}