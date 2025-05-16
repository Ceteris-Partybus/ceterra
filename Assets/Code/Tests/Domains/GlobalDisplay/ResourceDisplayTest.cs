using NUnit.Framework;
using System;

public class ResourceDisplayTest {
    [TestCase]
    public void Initializes() {
        ResourceDisplay resourceDisplay = new ResourceDisplay(10);
        Assert.AreEqual(0, resourceDisplay.MinValue);
        Assert.AreEqual(100, resourceDisplay.MaxValue);
        Assert.AreEqual(10, resourceDisplay.CurrentValue);
        Assert.AreEqual(DisplayTrend.Trend.stagnant, resourceDisplay.CurrentTrend);
    }

    [TestCase]
    public void Sets() {
        ResourceDisplay resourceDisplay = new ResourceDisplay(10);
        var ex = Assert.Throws<Exception>(() => resourceDisplay.CurrentValue = -1);
        Assert.That(ex.Message, Is.EqualTo("Value must be between " + resourceDisplay.MinValue + " and " + resourceDisplay.MaxValue));
        var ex1 = Assert.Throws<Exception>(() => resourceDisplay.CurrentValue = 101);
        Assert.That(ex1.Message, Is.EqualTo("Value must be between " + resourceDisplay.MinValue + " and " + resourceDisplay.MaxValue));
    }

    [TestCase]
    public void Adds() {
        ResourceDisplay resourceDisplay = new ResourceDisplay(0);
        resourceDisplay.AddCurrentValue(10);
        Assert.AreEqual(10, resourceDisplay.CurrentValue);
        resourceDisplay.AddCurrentValue(100);
        Assert.AreEqual(100, resourceDisplay.CurrentValue);
    }

    [TestCase]
    public void Subtracts() {
        ResourceDisplay resourceDisplay = new ResourceDisplay(10);
        resourceDisplay.SubtractCurrentValue(10);
        Assert.AreEqual(0, resourceDisplay.CurrentValue);
        var ex = Assert.Throws<Exception>(() => resourceDisplay.SubtractCurrentValue(100));
        Assert.That(ex.Message, Is.EqualTo("Subtraction exceeds minimum value " + resourceDisplay.MinValue));
    }

    [TestCase]
    public void CalculatesTrend() {
        ResourceDisplay resourceDisplay = new ResourceDisplay(10);
        resourceDisplay.CurrentValue = 20;
        resourceDisplay.IterateLatestValues();
        resourceDisplay.CalculateTrend();
        Assert.AreEqual(DisplayTrend.Trend.rising, resourceDisplay.CurrentTrend);
        resourceDisplay.CurrentValue = 10;
        resourceDisplay.CalculateTrend();
        Assert.AreEqual(DisplayTrend.Trend.falling, resourceDisplay.CurrentTrend);
        resourceDisplay.CurrentValue = 15;
        resourceDisplay.CalculateTrend();
        Assert.AreEqual(DisplayTrend.Trend.stagnant, resourceDisplay.CurrentTrend);
    }
}