using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Assets.Code.Scripts;

public class GlobalDisplayTest {
    // A Test behaves as an ordinary method
    [Test]
    public void NewTestScriptSimplePasses() {
        GlobalDisplay globalDisplay = new GlobalDisplay(10, 0, 20);
        Assert.AreEqual(10, globalDisplay.CurrentValue);
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
