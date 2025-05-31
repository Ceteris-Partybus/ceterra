using NUnit.Framework;
using System;
using UnityEngine;

public class EconomyDisplayTest {
    [TestCase(TestName = "erster Test")] //Möglichkeit zur Annotation, wird in Unity nicht angezeigt
    public void Initializes() {
        EconomyDisplay economyDisplay = new EconomyDisplay(10);
        Assert.AreEqual(0, economyDisplay.MinValue);
        Assert.AreEqual(100, economyDisplay.MaxValue);
        Assert.AreEqual(10, economyDisplay.CurrentValue);
        Assert.AreEqual(DisplayTrend.STAGNANT, economyDisplay.CurrentTrend);
    }

    [TestCase]
    public void Sets() {
        EconomyDisplay economyDisplay = new EconomyDisplay(10);
        var ex = Assert.Throws<Exception>(() => economyDisplay.CurrentValue = -1);
        Assert.That(ex.Message, Is.EqualTo("Value must be between " + economyDisplay.MinValue + " and " + economyDisplay.MaxValue));
        var ex1 = Assert.Throws<Exception>(() => economyDisplay.CurrentValue = 101);
        Assert.That(ex1.Message, Is.EqualTo("Value must be between " + economyDisplay.MinValue + " and " + economyDisplay.MaxValue));
    }

    [TestCase]
    public void Adds() {
        EconomyDisplay economyDisplay = new EconomyDisplay(0);
        economyDisplay.AddCurrentValue(10);
        Assert.AreEqual(10, economyDisplay.CurrentValue);
        economyDisplay.AddCurrentValue(100);
        Assert.AreEqual(100, economyDisplay.CurrentValue);
    }

    [TestCase]
    public void Subtracts() {
        EconomyDisplay economyDisplay = new EconomyDisplay(10);
        economyDisplay.SubtractCurrentValue(10);
        Assert.AreEqual(0, economyDisplay.CurrentValue);
        economyDisplay.SubtractCurrentValue(10);
        Assert.AreEqual(0, economyDisplay.CurrentValue);
    }

    [TestCase]
    public void CalculatesTrend() {
        EconomyDisplay economyDisplay = new EconomyDisplay(10);
        economyDisplay.CurrentValue = 20;
        economyDisplay.latestValues.IterateLatestValues(economyDisplay.CurrentValue);
        economyDisplay.CalculateTrend();
        Assert.AreEqual(DisplayTrend.RISING, economyDisplay.CurrentTrend);
        economyDisplay.CurrentValue = 10;
        economyDisplay.CalculateTrend();
        Assert.AreEqual(DisplayTrend.FALLING, economyDisplay.CurrentTrend);
        economyDisplay.CurrentValue = 15;
        economyDisplay.CalculateTrend();
        Assert.AreEqual(DisplayTrend.STAGNANT, economyDisplay.CurrentTrend);
    }
}