using Mirror;

public static class CatastropheEffectSerializer {
    private const byte WILDFIRE = 1;
    private const byte DROUGHT = 2;

    public static void WriteCatastropheEffect(this NetworkWriter writer, CatastropheEffect effect) {
        if (effect is Wildfire wildfire) {
            writer.WriteByte(WILDFIRE);
            writer.WriteInt(wildfire.RemainingRounds);
        }
        else {
            throw new System.InvalidOperationException($"Unknown catastrophe effect type: {effect.GetType()}");
        }
    }

    public static CatastropheEffect ReadCatastropheEffect(this NetworkReader reader) {
        byte type = reader.ReadByte();
        return type switch {
            WILDFIRE => new Wildfire(SkyboxManager.Instance) {
                RemainingRounds = reader.ReadInt()
            },
            _ => throw new System.InvalidOperationException($"Invalid catastrophe effect type: {type}")
        };
    }
}
