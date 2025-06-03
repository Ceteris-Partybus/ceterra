using System;

public class Investment {
    protected int id;
    protected string displayName;
    protected string description;
    protected int goal;
    protected bool fulfilled = false;
    protected int currentValue = 0;
    protected (InvestmentDisplay, InvestmentModifier, int) investmentType;

    public int Id {
        get; set;
    }

    public string DisplayName {
        get; set;
    }

    public string Description {
        get; set;
    }

    public int Goal {
        get; set;
    }

    public bool Fulfilled {
        get; set;
    }

    public int CurrentValue {
        get {
            return this.currentValue;
        }
        set {
            if (value < 0 || value > this.Goal) {
                throw new Exception("Value must be between 0 and " + this.Goal);
            }
            else {
                this.currentValue = value;
            }
        }
    }

    public (InvestmentDisplay, InvestmentModifier, int) InvestmentType {
        get; set;
    }

    public Investment(int id, string displayName, string description, int goal) {
        this.Id = id;
        this.DisplayName = displayName;
        this.Description = description;
        this.Goal = goal;
    }

    public void Invest(int add) {
        if (this.CurrentValue + add > this.Goal) {
            throw new Exception("Goal must not be exceeded");
        }
        else {
            this.CurrentValue += add;
            if (this.CurrentValue == this.Goal) {
                this.Fulfilled = true;
            }
        }
    }
}