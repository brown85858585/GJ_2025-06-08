using System.Collections;
using System.Collections.Generic;
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
