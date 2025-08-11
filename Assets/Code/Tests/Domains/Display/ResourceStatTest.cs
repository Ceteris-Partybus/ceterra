using NUnit.Framework;
using System;

public class ResourceStatTest {
    [TestCase]
    public void Initializes() {
        ResourceStat resourceDisplay = new ResourceStat(10);
        Assert.AreEqual(0, resourceDisplay.Base.MinValue);
        Assert.AreEqual(100, resourceDisplay.Base.MaxValue);
        Assert.AreEqual(10, resourceDisplay.Base.CurrentValue);
        Assert.AreEqual(StatTrend.STAGNANT, resourceDisplay.Base.CurrentTrend);
    }

    [TestCase]
    public void Sets() {
        ResourceStat resourceDisplay = new ResourceStat(10);
        var ex = Assert.Throws<Exception>(() => resourceDisplay.Base.CurrentValue = -1);
        Assert.That(ex.Message, Is.EqualTo("Value must be between " + resourceDisplay.Base.MinValue + " and " + resourceDisplay.Base.MaxValue));
        var ex1 = Assert.Throws<Exception>(() => resourceDisplay.Base.CurrentValue = 101);
        Assert.That(ex1.Message, Is.EqualTo("Value must be between " + resourceDisplay.Base.MinValue + " and " + resourceDisplay.Base.MaxValue));
    }

    [TestCase]
    public void Adds() {
        ResourceStat resourceDisplay = new ResourceStat(0);
        resourceDisplay.Base.AddCurrentValue(10);
        Assert.AreEqual(10, resourceDisplay.Base.CurrentValue);
        resourceDisplay.Base.AddCurrentValue(100);
        Assert.AreEqual(100, resourceDisplay.Base.CurrentValue);
    }

    [TestCase]
    public void Subtracts() {
        ResourceStat resourceDisplay = new ResourceStat(10);
        resourceDisplay.SubtractCurrentValue(10);
        Assert.AreEqual(0, resourceDisplay.Base.CurrentValue);
        var ex = Assert.Throws<Exception>(() => resourceDisplay.SubtractCurrentValue(100));
        Assert.That(ex.Message, Is.EqualTo("Subtraction exceeds minimum value " + resourceDisplay.Base.MinValue));
    }

    [TestCase]
    public void CalculatesTrend() {
        ResourceStat resourceDisplay = new ResourceStat(10);
        resourceDisplay.Base.CurrentValue = 20;
        resourceDisplay.Base.IterateLatestValues(resourceDisplay.Base.CurrentValue);
        resourceDisplay.Base.CalculateTrend();
        Assert.AreEqual(StatTrend.RISING, resourceDisplay.Base.CurrentTrend);
        resourceDisplay.Base.CurrentValue = 10;
        resourceDisplay.Base.CalculateTrend();
        Assert.AreEqual(StatTrend.FALLING, resourceDisplay.Base.CurrentTrend);
        resourceDisplay.Base.CurrentValue = 15;
        resourceDisplay.Base.CalculateTrend();
        Assert.AreEqual(StatTrend.STAGNANT, resourceDisplay.Base.CurrentTrend);
    }
}