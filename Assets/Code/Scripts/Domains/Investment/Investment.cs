using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

public class Investment {
    protected int id;
    protected string displayName;
    protected string description;
    protected int moneyGoal;
    protected int resourceGoal;
    protected int moneyCurrentValue = 0;
    protected bool fulfilled = false;
    protected (InvestmentDisplay, InvestmentModifier, int) investmentType;
    protected int cooldown;

    public int Id {
        get; set;
    }

    public string DisplayName {
        get; set;
    }

    public string Description {
        get; set;
    }

    public int MoneyGoal {
        get; set;
    }

    public int ResourceGoal {
        get; set;
    }

    public bool Fulfilled {
        get; set;
    }

    public int Cooldown {
        get; set;
    }

    public int MoneyCurrentValue {
        get {
            return this.moneyCurrentValue;
        }
        set {
            if (value < 0 || value > this.MoneyGoal) {
                throw new Exception("Value must be between 0 and " + this.MoneyGoal);
            }
            else {
                this.moneyCurrentValue = value;
            }
        }
    }

    public (InvestmentDisplay, InvestmentModifier, int) InvestmentType {
        get; set;
    }

    public Investment(int id, string displayName, string description, int moneyGoal, int resourceGoal, InvestmentDisplay investmentDisplay, InvestmentModifier investmentModifier, int investmentTypeInt, int cooldown) {
        this.Id = id;
        this.DisplayName = displayName;
        this.Description = description;
        this.MoneyGoal = moneyGoal;
        this.ResourceGoal = resourceGoal;
        this.InvestmentType = (investmentDisplay, investmentModifier, investmentTypeInt);
        this.Cooldown = cooldown;
    }

    public void InvestMoney(int add) {
        if (this.MoneyCurrentValue + add > this.MoneyGoal) {
            throw new Exception("Goal must not be exceeded");
        }
        else {
            this.MoneyCurrentValue += add;
            if (this.MoneyCurrentValue == this.MoneyGoal) {
                this.Fulfilled = true;
            }
        }
    }

    public void FulfillInvestment(ResourceDisplay resourceDisplay) {
        if (this.MoneyGoal == this.MoneyCurrentValue && this.resourceGoal <= resourceDisplay.CurrentValue) {
            this.Fulfilled = true;
        }
        else {
            throw new Exception("Missing money or resources to fulfill investment");
        }
    }

    public static List<Investment> LoadInvestmentsFromResources() {
        string jsonText = File.ReadAllText("Assets/Resources/Domains/Investment/InvestmentList.json");
        List<Investment> investmentList = JsonConvert.DeserializeObject<List<Investment>>(jsonText);
        return investmentList;
    }
}