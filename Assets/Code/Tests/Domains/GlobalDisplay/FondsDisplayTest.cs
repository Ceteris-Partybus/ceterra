using NUnit.Framework;
using System;

public class FundsDisplayTest {
    [TestCase]
    public void Initializes() {
        FundsDisplay fundsDisplay = new FundsDisplay(10);
        Assert.AreEqual(0, fundsDisplay.MinValue);
        Assert.AreEqual(2147483647, fundsDisplay.MaxValue);
        Assert.AreEqual(10, fundsDisplay.CurrentValue);
        Assert.AreEqual(DisplayTrend.STAGNANT, fundsDisplay.CurrentTrend);
    }

    [TestCase]
    public void Sets() {
        FundsDisplay fundsDisplay = new FundsDisplay(10);
        var ex = Assert.Throws<Exception>(() => fundsDisplay.CurrentValue = -1);
        Assert.That(ex.Message, Is.EqualTo("Value must be between " + fundsDisplay.MinValue + " and " + fundsDisplay.MaxValue));
    }

    [TestCase]
    public void Adds() {
        FundsDisplay fundsDisplay = new FundsDisplay(0);
        fundsDisplay.AddCurrentValue(10);
        Assert.AreEqual(10, fundsDisplay.CurrentValue);
        fundsDisplay.CurrentValue = fundsDisplay.MaxValue - 1;
        fundsDisplay.AddCurrentValue(10);
        Assert.AreEqual(fundsDisplay.CurrentValue, fundsDisplay.MaxValue);
    }

    [TestCase]
    public void Subtracts() {
        FundsDisplay fundsDisplay = new FundsDisplay(10);
        fundsDisplay.SubtractCurrentValue(10);
        Assert.AreEqual(0, fundsDisplay.CurrentValue);
        var ex = Assert.Throws<Exception>(() => fundsDisplay.SubtractCurrentValue(100));
        Assert.That(ex.Message, Is.EqualTo("Subtraction exceeds minimum value " + fundsDisplay.MinValue));
    }

    [TestCase]
    public void CalculatesTrend() {
        FundsDisplay fundsDisplay = new FundsDisplay(10);
        fundsDisplay.CurrentValue = 20;
        fundsDisplay.IterateLatestValues();
        fundsDisplay.CalculateTrend();
        Assert.AreEqual(DisplayTrend.RISING, fundsDisplay.CurrentTrend);
        fundsDisplay.CurrentValue = 10;
        fundsDisplay.CalculateTrend();
        Assert.AreEqual(DisplayTrend.FALLING, fundsDisplay.CurrentTrend);
        fundsDisplay.CurrentValue = 15;
        fundsDisplay.CalculateTrend();
        Assert.AreEqual(DisplayTrend.STAGNANT, fundsDisplay.CurrentTrend);
    }
}