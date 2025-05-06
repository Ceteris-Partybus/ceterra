using System;

public class FondsDisplay {

    private const int minValue = 0;

    private int currentValue;

    public int MinValue {
        get;
    }

    public int CurrentValue {
        get {
            return this.currentValue;
        }
        set {
            if (value < this.MinValue) {
                throw new Exception("Value must not be smaller then " + this.MinValue);
            }
            else {
                this.currentValue = value;
            }
        }
    }

    public FondsDisplay(int currentValue) {
        this.CurrentValue = currentValue;
    }

    public void AddCurrentValue(int add) {
        this.CurrentValue += add;
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