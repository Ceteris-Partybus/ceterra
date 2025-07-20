using NUnit.Framework;
using System;

public class MoneyDisplayTest {
    [TestCase]
    public void Initializes() {
        MoneyDisplay moneyDisplay = new MoneyDisplay(10);
        Assert.AreEqual(0, moneyDisplay.MinValue);
        Assert.AreEqual(100, moneyDisplay.MaxValue);
        Assert.AreEqual(10, moneyDisplay.CurrentValue);
        Assert.AreEqual(DisplayTrend.STAGNANT, moneyDisplay.CurrentTrend);
    }

    [TestCase]
    public void Sets() {
        MoneyDisplay moneyDisplay = new MoneyDisplay(10);
        var ex = Assert.Throws<Exception>(() => moneyDisplay.CurrentValue = -1);
        Assert.That(ex.Message, Is.EqualTo("Value must be between " + moneyDisplay.MinValue + " and " + moneyDisplay.MaxValue));
        var ex1 = Assert.Throws<Exception>(() => moneyDisplay.CurrentValue = 101);
        Assert.That(ex1.Message, Is.EqualTo("Value must be between " + moneyDisplay.MinValue + " and " + moneyDisplay.MaxValue));
    }

    [TestCase]
    public void Adds() {
        MoneyDisplay moneyDisplay = new MoneyDisplay(0);
        FundsDisplay fundsDisplay = new FundsDisplay(0);
        moneyDisplay.AddCurrentValue(10, fundsDisplay);
        Assert.AreEqual(10, moneyDisplay.CurrentValue);
        moneyDisplay.AddCurrentValue(100, fundsDisplay);
        Assert.AreEqual(100, moneyDisplay.CurrentValue);
        Assert.AreEqual(10, fundsDisplay.CurrentValue);
    }

    [TestCase]
    public void Subtracts() {
        MoneyDisplay moneyDisplay = new MoneyDisplay(10);
        moneyDisplay.SubtractCurrentValue(10);
        Assert.AreEqual(0, moneyDisplay.CurrentValue);
        var ex = Assert.Throws<Exception>(() => moneyDisplay.SubtractCurrentValue(100));
        Assert.That(ex.Message, Is.EqualTo("Subtraction exceeds minimum value " + moneyDisplay.MinValue));
    }

    [TestCase]
    public void CalculatesTrend() {
        MoneyDisplay moneyDisplay = new MoneyDisplay(10);
        moneyDisplay.CurrentValue = 20;
        moneyDisplay.latestValues.IterateLatestValues(moneyDisplay.CurrentValue);
        moneyDisplay.CalculateTrend();
        Assert.AreEqual(DisplayTrend.RISING, moneyDisplay.CurrentTrend);
        moneyDisplay.CurrentValue = 10;
        moneyDisplay.CalculateTrend();
        Assert.AreEqual(DisplayTrend.FALLING, moneyDisplay.CurrentTrend);
        moneyDisplay.CurrentValue = 15;
        moneyDisplay.CalculateTrend();
        Assert.AreEqual(DisplayTrend.STAGNANT, moneyDisplay.CurrentTrend);
    }
}