using System;

[Serializable]
public struct FundsStat {
    public GeneralStat Base;

    public FundsStat(int currentValue) {
        Base = new GeneralStat(currentValue, Int32.MaxValue);
    }

    public void SubtractCurrentValue(int subtract) {
        if (Base.CurrentValue - subtract < Base.MinValue) {
            throw new Exception("Subtraction exceeds minimum value " + Base.MinValue);
        }
        Base.CurrentValue = Base.CurrentValue - subtract;
    }

    public void AddCurrentValue(int add) {
        if (Base.MaxValue - Base.CurrentValue < add) {
            Base.CurrentValue = Base.MaxValue;
        }
        else {
            Base.CurrentValue = Base.CurrentValue + add;
        }
    }
}