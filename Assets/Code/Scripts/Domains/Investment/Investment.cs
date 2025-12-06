using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class Investment : IEquatable<Investment> {
    private static int nextId = 1;

    [SerializeField] public int id;
    [SerializeField] public long displayName;
    [SerializeField] public long description;
    [SerializeField] public int requiredMoney;
    [SerializeField] public int requiredResources;
    [SerializeField] public int currentMoney = 0;
    [SerializeField] public InvestmentType type;
    [SerializeField] public List<InvestmentModifier> modifier;
    [SerializeField] public int cooldown;
    [SerializeField] public bool fullyFinanced;
    [SerializeField] public bool inConstruction; // Investment is being constructed
    [SerializeField] public bool completed; // Investment is completed and its effects are active
    [SerializeField] public string iconPath;

    [JsonIgnore]
    public StyleBackground Icon => new StyleBackground(Resources.Load<Texture2D>(iconPath));

    // Mirror requires a default constructor
    public Investment() { }

    [JsonConstructor]
    private Investment(long displayName, long description, int requiredMoney, int requiredResources, InvestmentType type, List<InvestmentModifier> modifier, int cooldown, string iconPath) {
        this.id = nextId++;
        this.displayName = displayName;
        this.description = description;
        this.requiredMoney = requiredMoney;
        this.requiredResources = requiredResources;
        this.type = type;
        this.modifier = modifier;
        this.cooldown = cooldown;
        this.inConstruction = false;
        this.completed = false;
        this.iconPath = iconPath;
    }

    public void Tick() {
        if (inConstruction && !completed) {
            if (--cooldown == 0) {
                inConstruction = false;
            }
        }
    }

    public int Invest(int amount) {
        if (completed) {
            return 0;
        }

        int surplus = (int)(this.currentMoney + amount - this.requiredMoney);

        if (surplus >= 0) {
            this.currentMoney = this.requiredMoney;
            this.fullyFinanced = true;
        }
        else {
            this.currentMoney += amount;
        }
        return surplus;
    }

    public bool Equals(Investment other) {
        if (other == null) {
            return false;
        }

        return id == other.id;
    }

    public override bool Equals(object obj) {
        return Equals(obj as Investment);
    }

    public override int GetHashCode() {
        return id.GetHashCode();
    }

    public static List<Investment> LoadInvestmentsFromResources() {
        TextAsset jsonFile = Resources.Load<TextAsset>("Domains/Investment/InvestmentList");
        List<Investment> investmentList = JsonConvert.DeserializeObject<List<Investment>>(jsonFile.text);
        return investmentList;
    }
}