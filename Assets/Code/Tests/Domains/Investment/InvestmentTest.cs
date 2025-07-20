using NUnit.Framework;
using System;
using System.Collections.Generic;

public class InvestmentTest {
    [TestCase]
    public void CreatesInvestmentFromJson() {
        List<Investment> investments = Investment.LoadInvestmentsFromResources();
        Assert.AreEqual(1, investments[0].Id);
        Assert.AreEqual("Investment1", investments[0].DisplayName);
        Assert.AreEqual("Beschreibung des ersten Investments", investments[0].Description);
        Assert.AreEqual(100, investments[0].MoneyGoal);
        Assert.AreEqual(100, investments[0].ResourceGoal);
        Assert.AreEqual((InvestmentDisplay.SOCIETY, InvestmentModifier.DECREASES, 10), investments[0].InvestmentType);
        Assert.AreEqual(1, investments[0].Cooldown);
    }
}