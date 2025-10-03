using System;

[Serializable]
public class FundsHistoryEntry : IEquatable<FundsHistoryEntry> {
    public int amount;
    public HistoryEntryType type;
    public string source;
    public DateTime timestamp;

    public FundsHistoryEntry() {
        this.timestamp = DateTime.Now;
    }
    public FundsHistoryEntry(int amount, HistoryEntryType type, string source) {
        this.amount = amount;
        this.type = type;
        this.source = source;
        this.timestamp = DateTime.Now;
    }
    public bool Equals(FundsHistoryEntry other) {
        if (other == null) {
            return false;
        }

        return amount == other.amount && type == other.type && source == other.source;
    }

    public override int GetHashCode() {
        return HashCode.Combine(amount, type, source, timestamp);
    }
}