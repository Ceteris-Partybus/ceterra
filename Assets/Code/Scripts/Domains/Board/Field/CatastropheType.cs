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
    public static (int, int) GetEffects(this CatastropheType catastropheType) {
        return catastropheType switch {
            CatastropheType.VOLCANO => (50, 20),
            CatastropheType.NUCLEAR_EXPLOSION => (50, 20),
            CatastropheType.LANDSLIDE => (50, 20),
            CatastropheType.WILDFIRE => (50, 20),
            CatastropheType.GLACIER_MELT => (50, 20),
            CatastropheType.DROUGHT => (50, 20),
            CatastropheType.TSUNAMI => (50, 20),
            _ => throw new System.ArgumentOutOfRangeException(nameof(catastropheType), catastropheType, null)
        };
    }
}