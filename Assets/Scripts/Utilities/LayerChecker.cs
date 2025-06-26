using UnityEngine;

namespace Utilities
{
    public class LayerChecker
    {
        public static bool CheckLayerMask(GameObject obj, LayerMask layers)
        {
            if (((1 << obj.layer) & layers) != 0)
            {
                return true;
            }

            return false;
        }
    }
}