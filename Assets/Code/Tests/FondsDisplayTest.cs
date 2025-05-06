using NUnit.Framework;
using System;

public class FondsDisplayTest {
    [Test]
    public void InitializeDisplay() {
        FondsDisplay fondsDisplay = new FondsDisplay(0);
        Assert.AreEqual(0, fondsDisplay.MinValue);
        Assert.AreEqual(0, fondsDisplay.CurrentValue);
        var ex = Assert.Throws<Exception>(() => fondsDisplay.CurrentValue = -1);
        Assert.That(ex.Message, Is.EqualTo("Value must not be smaller then " + fondsDisplay.MinValue));
    }

    [Test]
    public void ChangeDisplay() {
        FondsDisplay fondsDisplay = new FondsDisplay(0);
        fondsDisplay.AddCurrentValue(10);
        Assert.AreEqual(10, fondsDisplay.CurrentValue);
        fondsDisplay.SubtractCurrentValue(10);
        Assert.AreEqual(0, fondsDisplay.MinValue);
        var ex = Assert.Throws<Exception>(() => fondsDisplay.SubtractCurrentValue(10));
        Assert.That(ex.Message, Is.EqualTo("Subtraction exceeds minimum value " + fondsDisplay.MinValue));
    }
}