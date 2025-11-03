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

    public static long GetDisplayName(this CatastropheType catastropheType) {
        return catastropheType switch {
            CatastropheType.VOLCANO => 56648143445786624,
            CatastropheType.NUCLEAR_EXPLOSION => 56648327022084096,
            CatastropheType.LANDSLIDE => 56648670216814592,
            CatastropheType.WILDFIRE => 56648926065164288,
            CatastropheType.GLACIER_MELT => 56649363690455040,
            CatastropheType.DROUGHT => 56649684747649024,
            CatastropheType.TSUNAMI => 56649910111797248,
            _ => throw new System.ArgumentOutOfRangeException(nameof(catastropheType), catastropheType, null)
        };
    }

    public static long GetDescription(this CatastropheType catastropheType) {
        return catastropheType switch {
            CatastropheType.VOLCANO => 56648230402097152,
            CatastropheType.NUCLEAR_EXPLOSION => 56648380902113280,
            CatastropheType.LANDSLIDE => 56648729599770624,
            CatastropheType.WILDFIRE => 56648972298977280,
            CatastropheType.GLACIER_MELT => 56649388873056256,
            CatastropheType.DROUGHT => 56649705719169024,
            CatastropheType.TSUNAMI => 56649934925299712,
            _ => throw new System.ArgumentOutOfRangeException(nameof(catastropheType), catastropheType, null)
        };
    }

    public static CatastropheEffect CreateEffect(this CatastropheType catastropheType) {
        return catastropheType switch {
            CatastropheType.WILDFIRE => new Wildfire(SkyboxManager.Instance),
            _ => throw new System.ArgumentOutOfRangeException(nameof(catastropheType), catastropheType, null)
        };
    }
}