public enum CatastropheType {
    ATOMIC_EXPLOSION,
    DROUGHT,
    EARTHQUAKE,
    WILDFIRE,
}

public static class CatastropheTypeExtensions {
    public static CatastropheEffect CreateEffect(this CatastropheType catastropheType) {
        return catastropheType switch {
            CatastropheType.ATOMIC_EXPLOSION => new AtomicExplosion(),
            CatastropheType.DROUGHT => new Drought(SkyboxManager.Instance),
            CatastropheType.EARTHQUAKE => new Earthquake(),
            CatastropheType.WILDFIRE => new Wildfire(SkyboxManager.Instance),
            _ => throw new System.ArgumentOutOfRangeException(nameof(catastropheType), catastropheType, null)
        };
    }
}