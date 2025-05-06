using NUnit.Framework;
using System;

public class EconomyDisplayTest {
    [Test]
    public void InitializeDisplay() {
        EconomyDisplay economyDisplay = new EconomyDisplay(10);
        Assert.AreEqual(0, economyDisplay.MinValue);
        Assert.AreEqual(100, economyDisplay.MaxValue);
        Assert.AreEqual(10, economyDisplay.CurrentValue);
        var ex = Assert.Throws<Exception>(() => economyDisplay.CurrentValue = -1);
        Assert.That(ex.Message, Is.EqualTo("Value must be between " + economyDisplay.MinValue + " and " + economyDisplay.MaxValue));
        var ex1 = Assert.Throws<Exception>(() => economyDisplay.CurrentValue = 101);
        Assert.That(ex1.Message, Is.EqualTo("Value must be between " + economyDisplay.MinValue + " and " + economyDisplay.MaxValue));
    }

    [Test]
    public void ChangeDisplay() {
        EconomyDisplay economyDisplay = new EconomyDisplay(0);
        economyDisplay.AddCurrentValue(10);
        Assert.AreEqual(10, economyDisplay.CurrentValue);
        economyDisplay.AddCurrentValue(100);
        Assert.AreEqual(100, economyDisplay.CurrentValue);
        economyDisplay.SubtractCurrentValue(10);
        Assert.AreEqual(90, economyDisplay.CurrentValue);
        var ex = Assert.Throws<Exception>(() => economyDisplay.SubtractCurrentValue(100));
        Assert.That(ex.Message, Is.EqualTo("Subtraction exceeds minimum value " + economyDisplay.MinValue));
    }
}