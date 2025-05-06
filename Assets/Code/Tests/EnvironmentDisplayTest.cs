using NUnit.Framework;
using System;

public class EnvironmentDisplayTest {
    [Test]
    public void InitializeDisplay() {
        EnvironmentDisplay environmentDisplay = new EnvironmentDisplay(10);
        Assert.AreEqual(0, environmentDisplay.MinValue);
        Assert.AreEqual(100, environmentDisplay.MaxValue);
        Assert.AreEqual(10, environmentDisplay.CurrentValue);
        var ex = Assert.Throws<Exception>(() => environmentDisplay.CurrentValue = -1);
        Assert.That(ex.Message, Is.EqualTo("Value must be between " + environmentDisplay.MinValue + " and " + environmentDisplay.MaxValue));
        var ex1 = Assert.Throws<Exception>(() => environmentDisplay.CurrentValue = 101);
        Assert.That(ex1.Message, Is.EqualTo("Value must be between " + environmentDisplay.MinValue + " and " + environmentDisplay.MaxValue));
    }

    [Test]
    public void ChangeDisplay() {
        EnvironmentDisplay environmentDisplay = new EnvironmentDisplay(0);
        environmentDisplay.AddCurrentValue(10);
        Assert.AreEqual(10, environmentDisplay.CurrentValue);
        environmentDisplay.AddCurrentValue(100);
        Assert.AreEqual(100, environmentDisplay.CurrentValue);
        environmentDisplay.SubtractCurrentValue(10);
        Assert.AreEqual(90, environmentDisplay.CurrentValue);
        var ex = Assert.Throws<Exception>(() => environmentDisplay.SubtractCurrentValue(100));
        Assert.That(ex.Message, Is.EqualTo("Subtraction exceeds minimum value " + environmentDisplay.MinValue));
    }
}