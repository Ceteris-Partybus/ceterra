using NUnit.Framework;
using System;
using UnityEngine;

public class EconomyStatTest {
    [TestCase(TestName = "erster Test")] //Möglichkeit zur Annotation, wird in Unity nicht angezeigt
    public void Initializes() {
        EconomyStat economyDisplay = new EconomyStat(10);
        Assert.AreEqual(0, economyDisplay.Base.MinValue);
        Assert.AreEqual(100, economyDisplay.Base.MaxValue);
        Assert.AreEqual(10, economyDisplay.Base.CurrentValue);
        Assert.AreEqual(StatTrend.STAGNANT, economyDisplay.Base.CurrentTrend);
    }

    [TestCase]
    public void Sets() {
        EconomyStat economyDisplay = new EconomyStat(10);
        var ex = Assert.Throws<Exception>(() => economyDisplay.Base.CurrentValue = -1);
        Assert.That(ex.Message, Is.EqualTo("Value must be between " + economyDisplay.Base.MinValue + " and " + economyDisplay.Base.MaxValue));
        var ex1 = Assert.Throws<Exception>(() => economyDisplay.Base.CurrentValue = 101);
        Assert.That(ex1.Message, Is.EqualTo("Value must be between " + economyDisplay.Base.MinValue + " and " + economyDisplay.Base.MaxValue));
    }

    [TestCase]
    public void Adds() {
        EconomyStat economyDisplay = new EconomyStat(0);
        economyDisplay.Base.AddCurrentValue(10);
        Assert.AreEqual(10, economyDisplay.Base.CurrentValue);
        economyDisplay.Base.AddCurrentValue(100);
        Assert.AreEqual(100, economyDisplay.Base.CurrentValue);
    }

    [TestCase]
    public void Subtracts() {
        EconomyStat economyDisplay = new EconomyStat(10);
        economyDisplay.Base.SubtractCurrentValue(10);
        Assert.AreEqual(0, economyDisplay.Base.CurrentValue);
        economyDisplay.Base.SubtractCurrentValue(10);
        Assert.AreEqual(0, economyDisplay.Base.CurrentValue);
    }

    [TestCase]
    public void CalculatesTrend() {
        EconomyStat economyDisplay = new EconomyStat(10);
        economyDisplay.Base.CurrentValue = 20;
        economyDisplay.Base.IterateLatestValues(economyDisplay.Base.CurrentValue);
        economyDisplay.Base.CalculateTrend();
        Assert.AreEqual(StatTrend.RISING, economyDisplay.Base.CurrentTrend);
        economyDisplay.Base.CurrentValue = 10;
        economyDisplay.Base.CalculateTrend();
        Assert.AreEqual(StatTrend.FALLING, economyDisplay.Base.CurrentTrend);
        economyDisplay.Base.CurrentValue = 15;
        economyDisplay.Base.CalculateTrend();
        Assert.AreEqual(StatTrend.STAGNANT, economyDisplay.Base.CurrentTrend);
    }
}