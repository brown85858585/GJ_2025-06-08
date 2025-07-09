using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Effects.PostProcess
{
    public partial class VolumeProfileSelector : MonoBehaviour
    {
        [SerializeField] private  Volume volume;
        [SerializeField] private ProfileEntry[] entries;

        private Dictionary<PostEffectProfile, VolumeProfile> profileDict;

        void Awake()
        {
            profileDict = new Dictionary<PostEffectProfile, VolumeProfile>();
            foreach (var entry in entries)
            {
                if (entry.profile != null)
                    profileDict[entry.key] = entry.profile;
            }
        }

        public void SetProfile(PostEffectProfile key)
        {
            if (profileDict.TryGetValue(key, out var profile))
            {
                volume.profile = profile;
            }
            else
            {
                Debug.LogWarning($"Profile for key '{key}' not found");
            }
        }
    }
}