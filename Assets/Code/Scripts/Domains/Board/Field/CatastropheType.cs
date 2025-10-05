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

    public static string GetDisplayName(this CatastropheType catastropheType) {
        return catastropheType switch {
            CatastropheType.VOLCANO => "Volcanic Eruption!",
            CatastropheType.NUCLEAR_EXPLOSION => "Nuclear Disaster!",
            CatastropheType.LANDSLIDE => "Devastating Landslide!",
            CatastropheType.WILDFIRE => "Raging Wildfire!",
            CatastropheType.GLACIER_MELT => "Glacier Collapse!",
            CatastropheType.DROUGHT => "Severe Drought!",
            CatastropheType.TSUNAMI => "Tsunami Warning!",
            _ => throw new System.ArgumentOutOfRangeException(nameof(catastropheType), catastropheType, null)
        };
    }

    public static string GetDescription(this CatastropheType catastropheType) {
        return catastropheType switch {
            CatastropheType.VOLCANO => "Molten lava and toxic ash devastate the surrounding area.",
            CatastropheType.NUCLEAR_EXPLOSION => "Radioactive fallout spreads across the region causing severe damage.",
            CatastropheType.LANDSLIDE => "Massive rocks and debris crash down, burying everything in their path.",
            CatastropheType.WILDFIRE => "Uncontrolled flames consume everything, leaving only ash behind.",
            CatastropheType.GLACIER_MELT => "Rising waters flood the landscape as ancient ice rapidly melts.",
            CatastropheType.DROUGHT => "Extreme heat and lack of water turn fertile land into barren wasteland.",
            CatastropheType.TSUNAMI => "Massive waves surge inland, destroying everything in their wake.",
            _ => throw new System.ArgumentOutOfRangeException(nameof(catastropheType), catastropheType, null)
        };
    }
}