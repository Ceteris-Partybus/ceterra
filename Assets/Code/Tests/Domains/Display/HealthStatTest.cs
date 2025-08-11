using NUnit.Framework;
using System;

public class HealthStatTest {
    [TestCase]
    public void Initializes() {
        HealthStat healthDisplay = new HealthStat(10);
        Assert.AreEqual(0, healthDisplay.Base.MinValue);
        Assert.AreEqual(100, healthDisplay.Base.MaxValue);
        Assert.AreEqual(10, healthDisplay.Base.CurrentValue);
        Assert.AreEqual(StatTrend.STAGNANT, healthDisplay.Base.CurrentTrend);
    }

    [TestCase]
    public void Sets() {
        HealthStat healthDisplay = new HealthStat(10);
        var ex = Assert.Throws<Exception>(() => healthDisplay.Base.CurrentValue = -1);
        Assert.That(ex.Message, Is.EqualTo("Value must be between " + healthDisplay.Base.MinValue + " and " + healthDisplay.Base.MaxValue));
        var ex1 = Assert.Throws<Exception>(() => healthDisplay.Base.CurrentValue = 101);
        Assert.That(ex1.Message, Is.EqualTo("Value must be between " + healthDisplay.Base.MinValue + " and " + healthDisplay.Base.MaxValue));
    }

    [TestCase]
    public void Adds() {
        HealthStat healthDisplay = new HealthStat(0);
        healthDisplay.Base.AddCurrentValue(10);
        Assert.AreEqual(10, healthDisplay.Base.CurrentValue);
        healthDisplay.Base.AddCurrentValue(100);
        Assert.AreEqual(100, healthDisplay.Base.CurrentValue);
    }

    [TestCase]
    public void Subtracts() {
        HealthStat healthDisplay = new HealthStat(10);
        healthDisplay.Base.SubtractCurrentValue(10);
        Assert.AreEqual(0, healthDisplay.Base.CurrentValue);
        healthDisplay.Base.SubtractCurrentValue(10);
        Assert.AreEqual(0, healthDisplay.Base.CurrentValue);
    }

    [TestCase]
    public void CalculatesTrend() {
        HealthStat healthDisplay = new HealthStat(10);
        healthDisplay.Base.CurrentValue = 20;
        healthDisplay.Base.IterateLatestValues(healthDisplay.Base.CurrentValue);
        healthDisplay.Base.CalculateTrend();
        Assert.AreEqual(StatTrend.RISING, healthDisplay.Base.CurrentTrend);
        healthDisplay.Base.CurrentValue = 10;
        healthDisplay.Base.CalculateTrend();
        Assert.AreEqual(StatTrend.FALLING, healthDisplay.Base.CurrentTrend);
        healthDisplay.Base.CurrentValue = 15;
        healthDisplay.Base.CalculateTrend();
        Assert.AreEqual(StatTrend.STAGNANT, healthDisplay.Base.CurrentTrend);
    }
}