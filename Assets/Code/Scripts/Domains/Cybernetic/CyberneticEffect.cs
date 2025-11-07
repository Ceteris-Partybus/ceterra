
using System;
using UnityEngine;

[Serializable]
public class CyberneticEffect : IEquatable<CyberneticEffect> {
    [SerializeField] public CyberneticEffectType source;
    [SerializeField] public CyberneticEffectType target;
    [SerializeField] public AnimationCurve effectCurve;
    [SerializeField] public float effectMultiplier; // Positive => increase, Negative => decrease
    [SerializeField] public string description;
    [SerializeField] public CyberneticRequirement requirement;

    public CyberneticEffect() { }

    public CyberneticEffect(
        CyberneticEffectType source,
        CyberneticEffectType target,
        AnimationCurve effectCurve,
        float effectMultiplier,
        string description,
        CyberneticRequirement requirement
    ) {
        this.source = source;
        this.target = target;
        this.effectCurve = effectCurve;
        this.effectMultiplier = effectMultiplier;
        this.description = description;
        this.requirement = requirement;
    }

    public bool Equals(CyberneticEffect other) {
        return source == other.source
            && target == other.target
            && requirement == other.requirement
            && effectMultiplier == other.effectMultiplier
            && effectCurve == other.effectCurve
            && description == other.description;
    }
}