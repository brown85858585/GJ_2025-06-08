using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.MiniGames.Park
{
    public class ParkLevelView : MonoBehaviour
    {
        [SerializeField] private List<RingView> rings;
        public List<RingView> Rings => rings;

        public event Action<int> OnRingEntered;

        private void Awake()
        {
            for (int i = 0; i < rings.Count; i++)
            {
                rings[i].Id = i;
            }
        }

        private void Start()
        {
            foreach (var ringView in rings)
            {
                ringView.ShowRing();
                ringView.OnEnteredTargetMask += HandleEnteredTargetMask;
            }
        }

        private void HandleEnteredTargetMask(int id)
        {
            rings[id].OnEnteredTargetMask -= HandleEnteredTargetMask;
            rings[id].HideRing();
            OnRingEntered?.Invoke(id);
        }

        private void OnDestroy()
        {
            foreach (var ringView in rings)
            {
                ringView.OnEnteredTargetMask -= HandleEnteredTargetMask;
            }
        }
    }
}