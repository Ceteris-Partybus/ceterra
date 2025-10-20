using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(Animator))]
public class CharacterAnimatorLoader : MonoBehaviour {
    public RuntimeAnimatorController baseController;
    public CharacterAnimationSet animationSet;

    void Awake() {
        if (animationSet == null || baseController == null) {
            Debug.LogWarning($"{name}: Missing animSet or baseController!");
            return;
        }

        var animator = GetComponent<Animator>();
        var overrideController = new AnimatorOverrideController(baseController);
        animator.runtimeAnimatorController = overrideController;

        var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        overrideController.GetOverrides(overrides);

        overrides = overrides
            .Where(pair => animationSet.AnimationClips.ContainsKey(pair.Key.name))
            .Select(pair => new KeyValuePair<AnimationClip, AnimationClip>(pair.Key, animationSet.AnimationClips[pair.Key.name]))
            .ToList();

        overrideController.ApplyOverrides(overrides);
    }
}
