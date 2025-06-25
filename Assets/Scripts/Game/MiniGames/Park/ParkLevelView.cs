using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.MiniGames.Park
{
    public class ParkLevelView : MonoBehaviour
    {
        [SerializeField] private List<CheckpointView> checkpointViews;
        [SerializeField] private Slider staminaSlider;
        public List<CheckpointView> CheckpointViews => checkpointViews;

        public event Action<int> OnRingEntered;
        public event Action OnStaminaChanged;

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

        private void FixedUpdate()
        {
            OnStaminaChanged?.Invoke();
        }

        public void UpdateStaminaView(int stamina)
        {
            //Debug.Log("Stamina: " + stamina);

            if (staminaSlider != null)
            {
                staminaSlider.value = stamina;
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