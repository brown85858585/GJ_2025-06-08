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
        [SerializeField] private float staminaMultiplyer = 40;
        public List<CheckpointView> CheckpointViews => checkpointViews;
        public float StaminaMultiplyer => staminaMultiplyer;
        
        private int _checkpointCounter;
        public int CheckpointCounter => _checkpointCounter;
        public event Action<int> OnRingEntered;
        public event Action OnStaminaChanged;
        
        public event Action OnMiniGameStart;

        private void Awake()
        {
            for (int i = 0; i < checkpointViews.Count; i++)
            {
                checkpointViews[i].Id = i;
            }
        }

        private void OnEnable()
        {
            InitRings();
            _checkpointCounter = CheckpointViews.Count;
            OnMiniGameStart?.Invoke();
        }

        private void OnDisable()
        {
            _checkpointCounter = 0;
            HideRings();
        }

        private void FixedUpdate()
        {
            OnStaminaChanged?.Invoke();
        }

        public void ResetRings()
        {
            InitRings();
        }

        private void InitRings()
        {
            foreach (var ringView in checkpointViews)
            {
                ringView.ShowRing();
                ringView.OnEnteredTargetMask += HandleEnteredTargetMask;
            }
        }

        private void HideRings()
        {
            foreach (var ringView in checkpointViews)
            {
                ringView.OnEnteredTargetMask -= HandleEnteredTargetMask;
                ringView.HideRing();
            }
        }

        public void UpdateStaminaView(int stamina)
        {
            if (staminaSlider != null)
            {
                staminaSlider.value = stamina;
            }
        }

        private void HandleEnteredTargetMask(int id)
        {
            _checkpointCounter--;
            
            checkpointViews[id].OnEnteredTargetMask -= HandleEnteredTargetMask;
            checkpointViews[id].HideRing();
            OnRingEntered?.Invoke(id);
            
        }
        
        public void SetStaminaMultiplyer(float value)
        {
            staminaMultiplyer = value;
        }

        private void OnDestroy()
        {
            HideRings();
        }
    }
}