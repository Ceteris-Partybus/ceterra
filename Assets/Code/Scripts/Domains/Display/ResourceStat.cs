using System;

[Serializable]
public struct ResourceStat {
    public GeneralStat Base;
    public ResourceStat(int currentValue) {
        Base = new GeneralStat(currentValue);
    }

    public void SubtractCurrentValue(int subtract) {
        if (Base.CurrentValue - subtract < Base.MinValue) {
            throw new Exception("Subtraction exceeds minimum value " + Base.MinValue);
        }
        Base.CurrentValue = Base.CurrentValue - subtract;
    }
}