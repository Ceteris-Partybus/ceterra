using NUnit.Framework;
using System;

public class FundsStatTest {
    [TestCase]
    public void Initializes() {
        FundsStat fundsDisplay = new FundsStat(10);
        Assert.AreEqual(0, fundsDisplay.Base.MinValue);
        Assert.AreEqual(2147483647, fundsDisplay.Base.MaxValue);
        Assert.AreEqual(10, fundsDisplay.Base.CurrentValue);
        Assert.AreEqual(StatTrend.STAGNANT, fundsDisplay.Base.CurrentTrend);
    }

    [TestCase]
    public void Sets() {
        FundsStat fundsDisplay = new FundsStat(10);
        var ex = Assert.Throws<Exception>(() => fundsDisplay.Base.CurrentValue = -1);
        Assert.That(ex.Message, Is.EqualTo("Value must be between " + fundsDisplay.Base.MinValue + " and " + fundsDisplay.Base.MaxValue));
    }

    [TestCase]
    public void Adds() {
        FundsStat fundsDisplay = new FundsStat(0);
        fundsDisplay.AddCurrentValue(10);
        Assert.AreEqual(10, fundsDisplay.Base.CurrentValue);
        fundsDisplay.Base.CurrentValue = fundsDisplay.Base.MaxValue - 1;
        fundsDisplay.AddCurrentValue(10);
        Assert.AreEqual(fundsDisplay.Base.CurrentValue, fundsDisplay.Base.MaxValue);
    }

    [TestCase]
    public void Subtracts() {
        FundsStat fundsDisplay = new FundsStat(10);
        fundsDisplay.SubtractCurrentValue(10);
        Assert.AreEqual(0, fundsDisplay.Base.CurrentValue);
        var ex = Assert.Throws<Exception>(() => fundsDisplay.SubtractCurrentValue(100));
        Assert.That(ex.Message, Is.EqualTo("Subtraction exceeds minimum value " + fundsDisplay.Base.MinValue));
    }

    [TestCase]
    public void CalculatesTrend() {
        FundsStat fundsDisplay = new FundsStat(10);
        fundsDisplay.Base.CurrentValue = 20;
        fundsDisplay.Base.IterateLatestValues(fundsDisplay.Base.CurrentValue);
        fundsDisplay.Base.CalculateTrend();
        Assert.AreEqual(StatTrend.RISING, fundsDisplay.Base.CurrentTrend);
        fundsDisplay.Base.CurrentValue = 10;
        fundsDisplay.Base.CalculateTrend();
        Assert.AreEqual(StatTrend.FALLING, fundsDisplay.Base.CurrentTrend);
        fundsDisplay.Base.CurrentValue = 15;
        fundsDisplay.Base.CalculateTrend();
        Assert.AreEqual(StatTrend.STAGNANT, fundsDisplay.Base.CurrentTrend);
    }
}