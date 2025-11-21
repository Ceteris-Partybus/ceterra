using Mirror;

public static class CatastropheEffectSerializer {
    public static void WriteCatastropheEffect(this NetworkWriter writer, CatastropheEffect effect) {
        writer.WriteByte((byte)effect.GetCatastropheType());
        writer.WriteInt(effect.RemainingRounds);
    }

    public static CatastropheEffect ReadCatastropheEffect(this NetworkReader reader) {
        var type = (CatastropheType)reader.ReadByte();
        var remainingRounds = reader.ReadInt();
        return type switch {
            CatastropheType.DROUGHT => new Drought(SkyboxManager.Instance) {
                RemainingRounds = remainingRounds
            },
            CatastropheType.EARTHQUAKE => new Earthquake() {
                RemainingRounds = remainingRounds
            },
            CatastropheType.WILDFIRE => new Wildfire(SkyboxManager.Instance) {
                RemainingRounds = remainingRounds
            },
            _ => throw new System.InvalidOperationException($"Invalid catastrophe effect type: {type}")
        };
    }
}