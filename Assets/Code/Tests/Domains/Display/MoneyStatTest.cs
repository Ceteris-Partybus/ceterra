using NUnit.Framework;
using System;

public class MoneyStatTest {
    [TestCase]
    public void Initializes() {
        MoneyStat moneyDisplay = new MoneyStat(10);
        Assert.AreEqual(0, moneyDisplay.Base.MinValue);
        Assert.AreEqual(100, moneyDisplay.Base.MaxValue);
        Assert.AreEqual(10, moneyDisplay.Base.CurrentValue);
        Assert.AreEqual(StatTrend.STAGNANT, moneyDisplay.Base.CurrentTrend);
    }

    [TestCase]
    public void Sets() {
        MoneyStat moneyDisplay = new MoneyStat(10);
        var ex = Assert.Throws<Exception>(() => moneyDisplay.Base.CurrentValue = -1);
        Assert.That(ex.Message, Is.EqualTo("Value must be between " + moneyDisplay.Base.MinValue + " and " + moneyDisplay.Base.MaxValue));
        var ex1 = Assert.Throws<Exception>(() => moneyDisplay.Base.CurrentValue = 101);
        Assert.That(ex1.Message, Is.EqualTo("Value must be between " + moneyDisplay.Base.MinValue + " and " + moneyDisplay.Base.MaxValue));
    }

    [TestCase]
    public void Adds() {
        MoneyStat moneyDisplay = new MoneyStat(0);
        FundsStat fundsDisplay = new FundsStat(0);
        moneyDisplay.AddCurrentValue(10, ref fundsDisplay);
        Assert.AreEqual(10, moneyDisplay.Base.CurrentValue);
        moneyDisplay.AddCurrentValue(100, ref fundsDisplay);
        Assert.AreEqual(100, moneyDisplay.Base.CurrentValue);
        Assert.AreEqual(10, fundsDisplay.Base.CurrentValue);
    }

    [TestCase]
    public void Subtracts() {
        MoneyStat moneyDisplay = new MoneyStat(10);
        moneyDisplay.SubtractCurrentValue(10);
        Assert.AreEqual(0, moneyDisplay.Base.CurrentValue);
        var ex = Assert.Throws<Exception>(() => moneyDisplay.SubtractCurrentValue(100));
        Assert.That(ex.Message, Is.EqualTo("Subtraction exceeds minimum value " + moneyDisplay.Base.MinValue));
    }

    [TestCase]
    public void CalculatesTrend() {
        MoneyStat moneyDisplay = new MoneyStat(10);
        moneyDisplay.Base.CurrentValue = 20;
        moneyDisplay.Base.IterateLatestValues(moneyDisplay.Base.CurrentValue);
        moneyDisplay.Base.CalculateTrend();
        Assert.AreEqual(StatTrend.RISING, moneyDisplay.Base.CurrentTrend);
        moneyDisplay.Base.CurrentValue = 10;
        moneyDisplay.Base.CalculateTrend();
        Assert.AreEqual(StatTrend.FALLING, moneyDisplay.Base.CurrentTrend);
        moneyDisplay.Base.CurrentValue = 15;
        moneyDisplay.Base.CalculateTrend();
        Assert.AreEqual(StatTrend.STAGNANT, moneyDisplay.Base.CurrentTrend);
    }
}