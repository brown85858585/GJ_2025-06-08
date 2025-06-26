using UnityEngine;

namespace Player
{
    public class OurHuman : MonoBehaviour
    {
        [SerializeField] private PlayerView playerView;

        public void WakeUpAnimationEnded()
        {
            playerView.WakeUpAnimationEnded();
        }
    }
}
