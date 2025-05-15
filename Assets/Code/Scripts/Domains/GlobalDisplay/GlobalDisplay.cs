using System;

public class GlobalDisplay {

    protected const int MIN_VALUE = 0;
    protected int MAX_VALUE = 100;
    protected int currentValue;

    public int MinValue {
        get {
            return MIN_VALUE;
        }
    }

    public int MaxValue {
        get {
            return this.MAX_VALUE;
        }
        set {
            this.MAX_VALUE = value;
        }
    }

    public int CurrentValue {
        get {
            return this.CurrentValue;
        }
        set {
            if (value < this.MinValue || value > this.MaxValue) {
                throw new Exception("Value must be between " + this.MinValue + " and " + this.MaxValue);
            }
            else {
                this.currentValue = value;
            }
        }
    }

    public void AddCurrentValue(int add) {
        if (this.CurrentValue + add > this.MaxValue) {
            this.CurrentValue = this.MaxValue;
        }
        else {
            this.CurrentValue += add;
        }
    }

    public virtual void SubtractCurrentValue(int subtract) {
        if (this.CurrentValue - subtract < this.MinValue) {
            this.CurrentValue = this.MinValue;
        }
        else {
            this.CurrentValue -= subtract;
        }
    }
}