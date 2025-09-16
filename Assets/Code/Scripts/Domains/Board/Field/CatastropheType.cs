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
            CatastropheType.VOLCANO => (-50, 20, 5),
            CatastropheType.NUCLEAR_EXPLOSION => (-50, 20, 5),
            CatastropheType.LANDSLIDE => (-50, 20, 5),
            CatastropheType.WILDFIRE => (-50, 20, 5),
            CatastropheType.GLACIER_MELT => (-50, 20, 5),
            CatastropheType.DROUGHT => (-50, 20, 5),
            CatastropheType.TSUNAMI => (-50, 20, 5),
            _ => throw new System.ArgumentOutOfRangeException(nameof(catastropheType), catastropheType, null)
        };
    }
}