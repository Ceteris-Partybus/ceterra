
using System;

[Serializable]
public struct EconomyStat {
    public GeneralStat Base;
    public EconomyStat(int currentValue) {
        Base = new GeneralStat(currentValue);
    }
}