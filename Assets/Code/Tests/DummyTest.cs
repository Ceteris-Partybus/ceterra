using NUnit.Framework;

public class DummyTest {
    [Test]
    public void alwaysPasses() {
        Assert.AreEqual(1, 1);
    }

    [Test]
    public void alwaysFails() {
        Assert.AreEqual(0, 1);
    }
}