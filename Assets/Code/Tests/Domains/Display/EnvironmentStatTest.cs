using NUnit.Framework;
using System;

public class EnvironmentStatTest {
    [TestCase]
    public void Initializes() {
        EnvironmentStat environmentDisplay = new EnvironmentStat(10);
        Assert.AreEqual(0, environmentDisplay.Base.MinValue);
        Assert.AreEqual(100, environmentDisplay.Base.MaxValue);
        Assert.AreEqual(10, environmentDisplay.Base.CurrentValue);
        Assert.AreEqual(StatTrend.STAGNANT, environmentDisplay.Base.CurrentTrend);
    }

    [TestCase]
    public void Sets() {
        EnvironmentStat environmentDisplay = new EnvironmentStat(10);
        var ex = Assert.Throws<Exception>(() => environmentDisplay.Base.CurrentValue = -1);
        Assert.That(ex.Message, Is.EqualTo("Value must be between " + environmentDisplay.Base.MinValue + " and " + environmentDisplay.Base.MaxValue));
        var ex1 = Assert.Throws<Exception>(() => environmentDisplay.Base.CurrentValue = 101);
        Assert.That(ex1.Message, Is.EqualTo("Value must be between " + environmentDisplay.Base.MinValue + " and " + environmentDisplay.Base.MaxValue));
    }

    [TestCase]
    public void Adds() {
        EnvironmentStat environmentDisplay = new EnvironmentStat(0);
        environmentDisplay.Base.AddCurrentValue(10);
        Assert.AreEqual(10, environmentDisplay.Base.CurrentValue);
        environmentDisplay.Base.AddCurrentValue(100);
        Assert.AreEqual(100, environmentDisplay.Base.CurrentValue);
    }

    [TestCase]
    public void Subtracts() {
        EnvironmentStat environmentDisplay = new EnvironmentStat(10);
        environmentDisplay.Base.SubtractCurrentValue(10);
        Assert.AreEqual(0, environmentDisplay.Base.CurrentValue);
        environmentDisplay.Base.SubtractCurrentValue(10);
        Assert.AreEqual(0, environmentDisplay.Base.CurrentValue);
    }

    [TestCase]
    public void CalculatesTrend() {
        EnvironmentStat environmentDisplay = new EnvironmentStat(10);
        environmentDisplay.Base.CurrentValue = 20;
        environmentDisplay.Base.IterateLatestValues(environmentDisplay.Base.CurrentValue);
        environmentDisplay.Base.CalculateTrend();
        Assert.AreEqual(StatTrend.RISING, environmentDisplay.Base.CurrentTrend);
        environmentDisplay.Base.CurrentValue = 10;
        environmentDisplay.Base.CalculateTrend();
        Assert.AreEqual(StatTrend.FALLING, environmentDisplay.Base.CurrentTrend);
        environmentDisplay.Base.CurrentValue = 15;
        environmentDisplay.Base.CalculateTrend();
        Assert.AreEqual(StatTrend.STAGNANT, environmentDisplay.Base.CurrentTrend);
    }
}