using System;

[Serializable]
public struct HealthStat {
    public GeneralStat Base;
    public HealthStat(int currentValue) {
        Base = new GeneralStat(currentValue);
    }
}