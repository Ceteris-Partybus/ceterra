using System;
using System.Collections.Generic;
using System.Linq;

public class GlobalDisplay {

    protected const int MIN_VALUE = 0;
    protected int maxValue = 100;
    protected int currentValue;
    protected List<int> latestValues = new();
    protected int listIterator = 1;
    protected DisplayTrend.Trend currentTrend;

    public int MinValue {
        get {
            return MIN_VALUE;
        }
    }

    public int MaxValue {
        get {
            return this.maxValue;
        }
        set {
            this.maxValue = value;
        }
    }

    public int CurrentValue {
        get {
            return this.currentValue;
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

    public DisplayTrend.Trend CurrentTrend {
        get; set;
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

    public void CalculateTrend() {
        double average = this.latestValues.Average();
        if (average > this.CurrentValue) {
            this.CurrentTrend = DisplayTrend.Trend.falling;
        }
        else if (average < this.CurrentValue) {
            this.CurrentTrend = DisplayTrend.Trend.rising;
        }
        else if (average == this.CurrentValue) {
            this.CurrentTrend = DisplayTrend.Trend.stagnant;
        }
        else {
            throw new Exception("Calculated average " + average + " could not be compared with the current value " + this.CurrentValue);
        }
    }

    public void IterateLatestValues() {
        if (this.listIterator < 5) {
            this.latestValues.Add(this.CurrentValue);
        }
        else {
            this.latestValues[this.listIterator % 5] = this.CurrentValue;
        }
        this.listIterator++;
    }
}