using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.MiniGames.Park
{
    public class ParkLevelView : MonoBehaviour
    {
        [SerializeField] private List<CheckpointView> checkpointViews;
        public List<CheckpointView> CheckpointViews => checkpointViews;

        public event Action<int> OnRingEntered;

        private void Awake()
        {
            for (int i = 0; i < checkpointViews.Count; i++)
            {
                checkpointViews[i].Id = i;
            }
        }

        private void Start()
        {
            foreach (var ringView in checkpointViews)
            {
                ringView.ShowRing();
                ringView.OnEnteredTargetMask += HandleEnteredTargetMask;
            }
        }

        private void HandleEnteredTargetMask(int id)
        {
            checkpointViews[id].OnEnteredTargetMask -= HandleEnteredTargetMask;
            checkpointViews[id].HideRing();
            OnRingEntered?.Invoke(id);
        }

        private void OnDestroy()
        {
            foreach (var ringView in checkpointViews)
            {
                ringView.OnEnteredTargetMask -= HandleEnteredTargetMask;
            }
        }
    }
}