public enum CatastropheType {
    DROUGHT,
    EARTHQUAKE,
    WILDFIRE,
}

public static class CatastropheTypeExtensions {
    public static CatastropheEffect CreateEffect(this CatastropheType catastropheType) {
        return catastropheType switch {
            CatastropheType.DROUGHT => new Drought(SkyboxManager.Instance),
            CatastropheType.EARTHQUAKE => new Earthquake(),
            CatastropheType.WILDFIRE => new Wildfire(SkyboxManager.Instance),
            _ => throw new System.ArgumentOutOfRangeException(nameof(catastropheType), catastropheType, null)
        };
    }
}