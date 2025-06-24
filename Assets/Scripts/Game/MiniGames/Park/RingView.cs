using System;
using UnityEngine;
using Utilities;

namespace Game.MiniGames.Park
{
    public class RingView : MonoBehaviour
    {
        public event Action<int> OnEnteredTargetMask;
        
        // LayerMask - Player
        private LayerMask _targetMask = 1 << 6;
        public int Id { get; set; }

        private void OnTriggerEnter(Collider other)
        {
            if (LayerChecker.CheckLayerMask(other.gameObject, _targetMask))
            {
                OnEnteredTargetMask?.Invoke(Id);
            }
        }

        public void HideRing()
        {
            gameObject.SetActive(false);
        }
        
        public void ShowRing()
        {
            gameObject.SetActive(true);
        }
    }
}