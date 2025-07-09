using UnityEngine.Rendering;

namespace Effects.PostProcess
{
    public partial class VolumeProfileSelector
    {
        [System.Serializable]
        public struct ProfileEntry
        {
            public PostEffectProfile key;
            public VolumeProfile profile;
        }
    }
}