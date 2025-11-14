using Mirror;

public static class CatastropheEffectSerializer {
    private const byte WILDFIRE = 1;
    private const byte DROUGHT = 2;

    public static void WriteCatastropheEffect(this NetworkWriter writer, CatastropheEffect effect) {
        var type = effect switch {
            Wildfire => WILDFIRE,
            Drought => DROUGHT,
            _ => throw new System.InvalidOperationException($"Unknown catastrophe effect type: {effect.GetType()}")
        };
        writer.WriteByte(type);
        writer.WriteInt(effect.RemainingRounds);
    }

    public static CatastropheEffect ReadCatastropheEffect(this NetworkReader reader) {
        var type = reader.ReadByte();
        var remainingRounds = reader.ReadInt();
        return type switch {
            WILDFIRE => new Wildfire(SkyboxManager.Instance) {
                RemainingRounds = remainingRounds
            },
            DROUGHT => new Drought(SkyboxManager.Instance) {
                RemainingRounds = remainingRounds
            },
            _ => throw new System.InvalidOperationException($"Invalid catastrophe effect type: {type}")
        };
    }
}