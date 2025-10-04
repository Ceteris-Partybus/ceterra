public enum CatastropheType {
    VOLCANO,
    NUCLEAR_EXPLOSION,
    LANDSLIDE,
    WILDFIRE,
    GLACIER_MELT,
    DROUGHT,
    TSUNAMI,
}

public static class CatastropheTypeExtensions {
    public static (int, int, int) GetEffects(this CatastropheType catastropheType) {
        return catastropheType switch {
            CatastropheType.VOLCANO => (-50, 20, 75),
            CatastropheType.NUCLEAR_EXPLOSION => (-50, 20, 75),
            CatastropheType.LANDSLIDE => (-50, 20, 75),
            CatastropheType.WILDFIRE => (-50, 20, 30),
            CatastropheType.GLACIER_MELT => (-50, 20, 50),
            CatastropheType.DROUGHT => (-50, 20, 100),
            CatastropheType.TSUNAMI => (-50, 20, 40),
            _ => throw new System.ArgumentOutOfRangeException(nameof(catastropheType), catastropheType, null)
        };
    }
}