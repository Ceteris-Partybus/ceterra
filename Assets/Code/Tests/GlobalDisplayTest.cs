using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System;

public class GlobalDisplayTest {
    // A Test behaves as an ordinary method
    [Test]
    public void InitializeDisplay() {
        GlobalDisplay globalDisplay = new GlobalDisplay(0, 100, 10);
        Assert.AreEqual(0, globalDisplay.MinValue);
        Assert.AreEqual(100, globalDisplay.MaxValue);
        Assert.AreEqual(10, globalDisplay.CurrentValue);
        var ex = Assert.Throws<Exception>(() => globalDisplay.CurrentValue = -1);
        Assert.That(ex.Message, Is.EqualTo("Value must be between " + globalDisplay.MinValue + " and " + globalDisplay.MaxValue));
        var ex1 = Assert.Throws<Exception>(() => globalDisplay.CurrentValue = 101);
        Assert.That(ex1.Message, Is.EqualTo("Value must be between " + globalDisplay.MinValue + " and " + globalDisplay.MaxValue));
    }

    [Test]
    public void ChangeDisplay() {
        GlobalDisplay globalDisplay = new GlobalDisplay(0, 100, 0);
        globalDisplay.AddCurrentValue(10);
        Assert.AreEqual(10, globalDisplay.CurrentValue);
        globalDisplay.AddCurrentValue(100);
        Assert.AreEqual(100, globalDisplay.CurrentValue);
        globalDisplay.SubtractCurrentValue(10);
        Assert.AreEqual(90, globalDisplay.CurrentValue);
        var ex = Assert.Throws<Exception>(() => globalDisplay.SubtractCurrentValue(100));
        Assert.That(ex.Message, Is.EqualTo("Subtraction exceeds minimum value " + globalDisplay.MinValue));
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator NewTestScriptWithEnumeratorPasses() {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;
    }
}
