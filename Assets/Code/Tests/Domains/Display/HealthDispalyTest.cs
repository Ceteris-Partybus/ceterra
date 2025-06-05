using NUnit.Framework;
using System;

public class HealthDisplayTest {
    [TestCase]
    public void Initializes() {
        HealthDisplay healthDisplay = new HealthDisplay(10);
        Assert.AreEqual(0, healthDisplay.MinValue);
        Assert.AreEqual(100, healthDisplay.MaxValue);
        Assert.AreEqual(10, healthDisplay.CurrentValue);
        Assert.AreEqual(DisplayTrend.STAGNANT, healthDisplay.CurrentTrend);
    }

    [TestCase]
    public void Sets() {
        HealthDisplay healthDisplay = new HealthDisplay(10);
        var ex = Assert.Throws<Exception>(() => healthDisplay.CurrentValue = -1);
        Assert.That(ex.Message, Is.EqualTo("Value must be between " + healthDisplay.MinValue + " and " + healthDisplay.MaxValue));
        var ex1 = Assert.Throws<Exception>(() => healthDisplay.CurrentValue = 101);
        Assert.That(ex1.Message, Is.EqualTo("Value must be between " + healthDisplay.MinValue + " and " + healthDisplay.MaxValue));
    }

    [TestCase]
    public void Adds() {
        HealthDisplay healthDisplay = new HealthDisplay(0);
        healthDisplay.AddCurrentValue(10);
        Assert.AreEqual(10, healthDisplay.CurrentValue);
        healthDisplay.AddCurrentValue(100);
        Assert.AreEqual(100, healthDisplay.CurrentValue);
    }

    [TestCase]
    public void Subtracts() {
        HealthDisplay healthDisplay = new HealthDisplay(10);
        healthDisplay.SubtractCurrentValue(10);
        Assert.AreEqual(0, healthDisplay.CurrentValue);
        healthDisplay.SubtractCurrentValue(10);
        Assert.AreEqual(0, healthDisplay.CurrentValue);
    }

    [TestCase]
    public void CalculatesTrend() {
        HealthDisplay healthDisplay = new HealthDisplay(10);
        healthDisplay.CurrentValue = 20;
        healthDisplay.latestValues.IterateLatestValues(healthDisplay.CurrentValue);
        healthDisplay.CalculateTrend();
        Assert.AreEqual(DisplayTrend.RISING, healthDisplay.CurrentTrend);
        healthDisplay.CurrentValue = 10;
        healthDisplay.CalculateTrend();
        Assert.AreEqual(DisplayTrend.FALLING, healthDisplay.CurrentTrend);
        healthDisplay.CurrentValue = 15;
        healthDisplay.CalculateTrend();
        Assert.AreEqual(DisplayTrend.STAGNANT, healthDisplay.CurrentTrend);
    }
}