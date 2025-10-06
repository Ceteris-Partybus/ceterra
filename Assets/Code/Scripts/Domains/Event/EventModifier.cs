using Newtonsoft.Json;
using System;

[Serializable]
public class EventModifier : IEquatable<EventModifier> {
    private EventType type;
    private EventEffect effect;
    private int magnitude;

    public EventType Type => type;
    public EventEffect Effect => effect;
    public int Magnitude => magnitude;

    public EventModifier() { }

    [JsonConstructor]
    public EventModifier(EventType type, EventEffect effect, int magnitude) {
        this.type = type;
        this.effect = effect;
        this.magnitude = magnitude;
    }

    public bool Equals(EventModifier other) {
        if (other == null) {
            return false;
        }

        return type == other.type && effect == other.effect && magnitude == other.magnitude;
    }
}