using NUnit.Framework;
using System;

public class SocietyDisplayTest {
    [Test]
    public void InitializeDisplay() {
        SocietyDisplay societyDisplay = new SocietyDisplay(10);
        Assert.AreEqual(0, societyDisplay.MinValue);
        Assert.AreEqual(100, societyDisplay.MaxValue);
        Assert.AreEqual(10, societyDisplay.CurrentValue);
        var ex = Assert.Throws<Exception>(() => societyDisplay.CurrentValue = -1);
        Assert.That(ex.Message, Is.EqualTo("Value must be between " + societyDisplay.MinValue + " and " + societyDisplay.MaxValue));
        var ex1 = Assert.Throws<Exception>(() => societyDisplay.CurrentValue = 101);
        Assert.That(ex1.Message, Is.EqualTo("Value must be between " + societyDisplay.MinValue + " and " + societyDisplay.MaxValue));
    }

    [Test]
    public void ChangeDisplay() {
        SocietyDisplay societyDisplay = new SocietyDisplay(0);
        societyDisplay.AddCurrentValue(10);
        Assert.AreEqual(10, societyDisplay.CurrentValue);
        societyDisplay.AddCurrentValue(100);
        Assert.AreEqual(100, societyDisplay.CurrentValue);
        societyDisplay.SubtractCurrentValue(10);
        Assert.AreEqual(90, societyDisplay.CurrentValue);
        var ex = Assert.Throws<Exception>(() => societyDisplay.SubtractCurrentValue(100));
        Assert.That(ex.Message, Is.EqualTo("Subtraction exceeds minimum value " + societyDisplay.MinValue));
    }
}