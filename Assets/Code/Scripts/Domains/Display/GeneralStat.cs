using System;
using UnityEngine;

[Serializable]
public struct GeneralStat {
    private const int MIN_VALUE = 0;
    private int maxValue;
    [SerializeField]
    private int currentValue;
    public const int HistoryLength = 5;
    [SerializeField]
    public int[] latestValues;
    private int historyCount;
    private int nextWriteIndex;
    [SerializeField]
    private StatTrend currentTrend;

    public int MinValue => MIN_VALUE;

    public int MaxValue {
        readonly get => maxValue;
        set => maxValue = value;
    }

    public int CurrentValue {
        readonly get => currentValue;
        set {
            if (value < MinValue || value > MaxValue) {
                throw new Exception("Value must be between " + MinValue + " and " + MaxValue);
            }
            currentValue = value;
        }
    }

    public StatTrend CurrentTrend {
        readonly get => currentTrend;
        private set => currentTrend = value;
    }

    public GeneralStat(int currentValue, int maxValue = 100) {
        this.maxValue = maxValue;
        this.currentValue = 0;
        latestValues = new int[HistoryLength];
        historyCount = 0;
        nextWriteIndex = 0;
        currentTrend = StatTrend.STAGNANT;
        CurrentValue = currentValue;
        IterateLatestValues(currentValue);
    }

    public void AddCurrentValue(int add) {
        if (CurrentValue + add > MaxValue) {
            CurrentValue = MaxValue;
        }
        else {
            CurrentValue = CurrentValue + add;
        }
        IterateLatestValues(CurrentValue);
    }

    public void SubtractCurrentValue(int subtract) {
        if (CurrentValue - subtract < MinValue) {
            CurrentValue = MinValue;
        }
        else {
            CurrentValue = CurrentValue - subtract;
        }
        IterateLatestValues(CurrentValue);
    }

    public void CalculateTrend() {
        if (historyCount == 0) {
            CurrentTrend = StatTrend.STAGNANT;
            return;
        }
        double sum = 0;
        for (int i = 0; i < historyCount; i++) {
            sum += latestValues[i];
        }
        double average = sum / historyCount;
        if (average > CurrentValue) {
            CurrentTrend = StatTrend.FALLING;
        }
        else if (average < CurrentValue) {
            CurrentTrend = StatTrend.RISING;
        }
        else {
            CurrentTrend = StatTrend.STAGNANT;
        }
    }

    public void IterateLatestValues(int value) {
        if (historyCount < HistoryLength) {
            latestValues[historyCount] = value;
            historyCount++;
        }
        else {
            latestValues[nextWriteIndex] = value;
            nextWriteIndex = (nextWriteIndex + 1) % HistoryLength;
        }
    }
}