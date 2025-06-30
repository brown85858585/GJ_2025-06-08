using System;
using UnityEngine;
using Utilities;

namespace Game.MiniGames.Park
{
    public class CheckpointView : MonoBehaviour
    {
        [SerializeField] private ParticleSystem particleSphere;
        [SerializeField] private GameObject sphere;
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
            particleSphere.gameObject.SetActive(false);
            sphere.gameObject.SetActive(false);
        }
        
        public void ShowRing()
        {
            particleSphere.gameObject.SetActive(true);
            sphere.gameObject.SetActive(true);
        }
    }
}