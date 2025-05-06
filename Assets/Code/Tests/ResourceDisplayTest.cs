using NUnit.Framework;
using System;

public class ResourcesDisplayTest {
    [Test]
    public void InitializeDisplay() {
        ResourcesDisplay resourcesDisplay = new ResourcesDisplay(0);
        Assert.AreEqual(0, resourcesDisplay.MinValue);
        Assert.AreEqual(0, resourcesDisplay.CurrentValue);
        var ex = Assert.Throws<Exception>(() => resourcesDisplay.CurrentValue = -1);
        Assert.That(ex.Message, Is.EqualTo("Value must not be smaller then " + resourcesDisplay.MinValue));
    }

    [Test]
    public void ChangeDisplay() {
        ResourcesDisplay resourcesDisplay = new ResourcesDisplay(0);
        resourcesDisplay.AddCurrentValue(10);
        Assert.AreEqual(10, resourcesDisplay.CurrentValue);
        resourcesDisplay.SubtractCurrentValue(10);
        Assert.AreEqual(0, resourcesDisplay.MinValue);
        var ex = Assert.Throws<Exception>(() => resourcesDisplay.SubtractCurrentValue(10));
        Assert.That(ex.Message, Is.EqualTo("Subtraction exceeds minimum value " + resourcesDisplay.MinValue));
    }
}