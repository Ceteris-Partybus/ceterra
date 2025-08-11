using System;

[Serializable]
public struct MoneyStat {
    public GeneralStat Base;
    public MoneyStat(int currentValue) {
        Base = new GeneralStat(currentValue);
    }

    public void AddCurrentValue(int add, ref FundsStat fundsStat) {
        if (Base.CurrentValue + add > Base.MaxValue) {
            int diff = Base.CurrentValue + add - Base.MaxValue;
            fundsStat.AddCurrentValue(diff);
            Base.CurrentValue = Base.MaxValue;
        }
        else {
            Base.CurrentValue = Base.CurrentValue + add;
        }
    }

    public void SubtractCurrentValue(int subtract) {
        if (Base.CurrentValue - subtract < Base.MinValue) {
            throw new Exception("Subtraction exceeds minimum value " + Base.MinValue);
        }
        Base.CurrentValue = Base.CurrentValue - subtract;
    }
}