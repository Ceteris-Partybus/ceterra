using NUnit.Framework;
using System;

public class SocietyStatTest {
    [TestCase]
    public void Initializes() {
        SocietyStat societyDisplay = new SocietyStat(10);
        Assert.AreEqual(0, societyDisplay.Base.MinValue);
        Assert.AreEqual(100, societyDisplay.Base.MaxValue);
        Assert.AreEqual(10, societyDisplay.Base.CurrentValue);
        Assert.AreEqual(StatTrend.STAGNANT, societyDisplay.Base.CurrentTrend);
    }

    [TestCase]
    public void Sets() {
        SocietyStat societyDisplay = new SocietyStat(10);
        var ex = Assert.Throws<Exception>(() => societyDisplay.Base.CurrentValue = -1);
        Assert.That(ex.Message, Is.EqualTo("Value must be between " + societyDisplay.Base.MinValue + " and " + societyDisplay.Base.MaxValue));
        var ex1 = Assert.Throws<Exception>(() => societyDisplay.Base.CurrentValue = 101);
        Assert.That(ex1.Message, Is.EqualTo("Value must be between " + societyDisplay.Base.MinValue + " and " + societyDisplay.Base.MaxValue));
    }

    [TestCase]
    public void Adds() {
        SocietyStat societyDisplay = new SocietyStat(0);
        societyDisplay.Base.AddCurrentValue(10);
        Assert.AreEqual(10, societyDisplay.Base.CurrentValue);
        societyDisplay.Base.AddCurrentValue(100);
        Assert.AreEqual(100, societyDisplay.Base.CurrentValue);
    }

    [TestCase]
    public void Subtracts() {
        SocietyStat societyDisplay = new SocietyStat(10);
        societyDisplay.Base.SubtractCurrentValue(10);
        Assert.AreEqual(0, societyDisplay.Base.CurrentValue);
        societyDisplay.Base.SubtractCurrentValue(10);
        Assert.AreEqual(0, societyDisplay.Base.CurrentValue);
    }

    [TestCase]
    public void CalculatesTrend() {
        SocietyStat societyDisplay = new SocietyStat(10);
        societyDisplay.Base.CurrentValue = 20;
        societyDisplay.Base.IterateLatestValues(societyDisplay.Base.CurrentValue);
        societyDisplay.Base.CalculateTrend();
        Assert.AreEqual(StatTrend.RISING, societyDisplay.Base.CurrentTrend);
        societyDisplay.Base.CurrentValue = 10;
        societyDisplay.Base.CalculateTrend();
        Assert.AreEqual(StatTrend.FALLING, societyDisplay.Base.CurrentTrend);
        societyDisplay.Base.CurrentValue = 15;
        societyDisplay.Base.CalculateTrend();
        Assert.AreEqual(StatTrend.STAGNANT, societyDisplay.Base.CurrentTrend);
    }
}