using System;

[Serializable]
public struct SocietyStat {
    public GeneralStat Base;
    public SocietyStat(int currentValue) {
        Base = new GeneralStat(currentValue);
    }
}