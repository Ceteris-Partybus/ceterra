using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CharacterAnimatorLoader : MonoBehaviour {
    public RuntimeAnimatorController baseController;

    [System.Serializable]
    public class ClipEntry {
        public string name;
        public AnimationClip clip;

        public ClipEntry(string name, AnimationClip clip) {
            this.name = name;
            this.clip = clip;
        }
    }

    public List<ClipEntry> animationClips = new List<ClipEntry>();

    void Awake() {
        if (animationClips == null || animationClips.Count == 0 || baseController == null) {
            Debug.LogWarning($"{name}: Missing animationClips or baseController!");
            return;
        }

        var animator = gameObject.AddComponent<Animator>();
        var overrideController = new AnimatorOverrideController(baseController);
        animator.runtimeAnimatorController = overrideController;

        var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        overrideController.GetOverrides(overrides);

        var clips = animationClips.ToDictionary(entry => entry.name, entry => entry.clip);

        overrides = overrides
            .Where(pair => clips.ContainsKey(pair.Key.name))
            .Select(pair => new KeyValuePair<AnimationClip, AnimationClip>(pair.Key, clips[pair.Key.name]))
            .ToList();

        overrideController.ApplyOverrides(overrides);
    }
}
