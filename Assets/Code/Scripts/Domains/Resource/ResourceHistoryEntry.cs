using System;

[Serializable]
public class ResourceHistoryEntry : IEquatable<ResourceHistoryEntry> {
    public int amount;
    public HistoryEntryType type;
    public string source;
    public DateTime timestamp;

    public ResourceHistoryEntry() {
        this.timestamp = DateTime.Now;
    }

    public ResourceHistoryEntry(int amount, HistoryEntryType type, string source) {
        this.amount = amount;
        this.type = type;
        this.source = source;
        this.timestamp = DateTime.Now;
    }

    public bool Equals(ResourceHistoryEntry other) {
        if (other == null) {
            return false;
        }

        return amount == other.amount && type == other.type && source == other.source;
    }

    public override int GetHashCode() {
        return HashCode.Combine(amount, type, source, timestamp);
    }
}