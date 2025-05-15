using NUnit.Framework;
using System;

public class EnvironmentDisplayTest {
    [TestCase]
    public void Initializes() {
        EnvironmentDisplay environmentDisplay = new EnvironmentDisplay(10);
        Assert.AreEqual(0, environmentDisplay.MinValue);
        Assert.AreEqual(100, environmentDisplay.MaxValue);
        Assert.AreEqual(10, environmentDisplay.CurrentValue);
    }

    [TestCase]
    public void Sets() {
        EnvironmentDisplay environmentDisplay = new EnvironmentDisplay(10);
        var ex = Assert.Throws<Exception>(() => environmentDisplay.CurrentValue = -1);
        Assert.That(ex.Message, Is.EqualTo("Value must be between " + environmentDisplay.MinValue + " and " + environmentDisplay.MaxValue));
        var ex1 = Assert.Throws<Exception>(() => environmentDisplay.CurrentValue = 101);
        Assert.That(ex1.Message, Is.EqualTo("Value must be between " + environmentDisplay.MinValue + " and " + environmentDisplay.MaxValue));
    }

    [TestCase]
    public void Adds() {
        EnvironmentDisplay environmentDisplay = new EnvironmentDisplay(0);
        environmentDisplay.AddCurrentValue(10);
        Assert.AreEqual(10, environmentDisplay.CurrentValue);
        environmentDisplay.AddCurrentValue(100);
        Assert.AreEqual(100, environmentDisplay.CurrentValue);
    }

    [TestCase]
    public void Subtracts() {
        EnvironmentDisplay environmentDisplay = new EnvironmentDisplay(10);
        environmentDisplay.SubtractCurrentValue(10);
        Assert.AreEqual(0, environmentDisplay.CurrentValue);
        environmentDisplay.SubtractCurrentValue(10);
        Assert.AreEqual(0, environmentDisplay.CurrentValue);
    }
}