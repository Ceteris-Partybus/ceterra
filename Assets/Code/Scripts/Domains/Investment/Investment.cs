using Mirror;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class Investment : IEquatable<Investment> {
    private static int nextId = 1;

    [SerializeField] public int id;
    [SerializeField] public string displayName;
    [SerializeField] public string description;
    [SerializeField] public uint requiredMoney;
    [SerializeField] public uint requiredResources;
    [SerializeField] public uint currentMoney = 0;
    [SerializeField] public InvestmentType type;
    [SerializeField] public List<InvestmentModifier> modifier;
    [SerializeField] public int cooldown;
    [SerializeField] public bool fullyFinanced;
    [SerializeField] public bool inConstruction; // Investment is being constructed
    [SerializeField] public bool completed; // Investment is completed and its effects are active

    // Mirror requires a default constructor
    public Investment() { }

    [JsonConstructor]
    private Investment(string displayName, string description, uint requiredMoney, uint requiredResources, InvestmentType type, List<InvestmentModifier> modifier, int cooldown) {
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
    }

    public void Tick() {
        if (inConstruction && !completed) {
            if (--cooldown == 0) {
                inConstruction = false;
            }
        }
    }

    public int Invest(uint amount) {
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