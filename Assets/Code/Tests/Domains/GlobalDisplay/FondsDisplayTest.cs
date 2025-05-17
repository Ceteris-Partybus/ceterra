using NUnit.Framework;
using System;

public class FondsDisplayTest {
    [TestCase]
    public void Initializes() {
        FondsDisplay fondsDisplay = new FondsDisplay(10);
        Assert.AreEqual(0, fondsDisplay.MinValue);
        Assert.AreEqual(2147483647, fondsDisplay.MaxValue);
        Assert.AreEqual(10, fondsDisplay.CurrentValue);
        Assert.AreEqual(DisplayTrend.Trend.stagnant, fondsDisplay.CurrentTrend);
    }

    [TestCase]
    public void Sets() {
        FondsDisplay fondsDisplay = new FondsDisplay(10);
        var ex = Assert.Throws<Exception>(() => fondsDisplay.CurrentValue = -1);
        Assert.That(ex.Message, Is.EqualTo("Value must be between " + fondsDisplay.MinValue + " and " + fondsDisplay.MaxValue));
    }

    [TestCase]
    public void Adds() {
        FondsDisplay fondsDisplay = new FondsDisplay(0);
        fondsDisplay.AddCurrentValue(10);
        Assert.AreEqual(10, fondsDisplay.CurrentValue);
    }

    [TestCase]
    public void Subtracts() {
        FondsDisplay fondsDisplay = new FondsDisplay(10);
        fondsDisplay.SubtractCurrentValue(10);
        Assert.AreEqual(0, fondsDisplay.CurrentValue);
        var ex = Assert.Throws<Exception>(() => fondsDisplay.SubtractCurrentValue(100));
        Assert.That(ex.Message, Is.EqualTo("Subtraction exceeds minimum value " + fondsDisplay.MinValue));
    }

    [TestCase]
    public void CalculatesTrend() {
        FondsDisplay fondsDisplay = new FondsDisplay(10);
        fondsDisplay.CurrentValue = 20;
        fondsDisplay.IterateLatestValues();
        fondsDisplay.CalculateTrend();
        Assert.AreEqual(DisplayTrend.Trend.rising, fondsDisplay.CurrentTrend);
        fondsDisplay.CurrentValue = 10;
        fondsDisplay.CalculateTrend();
        Assert.AreEqual(DisplayTrend.Trend.falling, fondsDisplay.CurrentTrend);
        fondsDisplay.CurrentValue = 15;
        fondsDisplay.CalculateTrend();
        Assert.AreEqual(DisplayTrend.Trend.stagnant, fondsDisplay.CurrentTrend);
    }
}