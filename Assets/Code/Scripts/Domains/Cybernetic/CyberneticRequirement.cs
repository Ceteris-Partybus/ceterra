using System;
using UnityEngine;

[Serializable]
public class CyberneticRequirement {
    [SerializeField] public int minValue;
    [SerializeField] public int maxValue;

    public CyberneticRequirement() { }

    public CyberneticRequirement(int minValue, int maxValue) {
        this.minValue = minValue;
        this.maxValue = maxValue;
    }
}