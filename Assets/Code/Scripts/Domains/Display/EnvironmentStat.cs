using System;

[Serializable]
public struct EnvironmentStat {
    public GeneralStat Base;
    public EnvironmentStat(int currentValue) {
        Base = new GeneralStat(currentValue);
    }
}