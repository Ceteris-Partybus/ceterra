using Newtonsoft.Json;
using System;

[Serializable]
public class InvestmentModifier : IEquatable<InvestmentModifier> {
    private InvestmentType type;
    private InvestmentEffect effect;
    private int magnitude;

    public InvestmentType Type => type;
    public InvestmentEffect Effect => effect;
    public int Magnitude => magnitude;

    public InvestmentModifier() { }

    [JsonConstructor]
    public InvestmentModifier(InvestmentType type, InvestmentEffect effect, int magnitude) {
        this.type = type;
        this.effect = effect;
        this.magnitude = magnitude;
    }

    public bool Equals(InvestmentModifier other) {
        if (other == null) {
            return false;
        }

        return type == other.type && effect == other.effect && magnitude == other.magnitude;
    }
}