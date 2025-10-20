using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "CharacterAnimationSet", menuName = "Characters/Generic Animation Set (Dynamic)")]
public class CharacterAnimationSet : ScriptableObject {
    [Tooltip("FBX asset that contains this character's animation clips")]
    public GameObject fbxAsset;

    [System.Serializable]
    public class ClipEntry {
        public string name;
        public AnimationClip clip;
    }

    public List<ClipEntry> clips = new List<ClipEntry>();

    private Dictionary<string, AnimationClip> animationClips;
    public Dictionary<string, AnimationClip> AnimationClips {
        get {
            animationClips ??= clips.ToDictionary(entry => entry.name, entry => entry.clip);
            return animationClips;
        }
    }
}
