using NUnit.Framework;

public class CylicListTest {

    [TestCase]
    public void Iterates() {
        CyclicList<int> cyclicList = new();
        cyclicList.ListLength = 2;
        cyclicList.IterateLatestValues(0);
        Assert.AreEqual(0, cyclicList[0]);
        cyclicList.IterateLatestValues(1);
        Assert.AreEqual(1, cyclicList[1]);
        cyclicList.IterateLatestValues(2);
        Assert.AreEqual(2, cyclicList[0]);
    }
}