using System.Collections.Generic;
using Game.MiniGames.Park;
using UI.Flower;
using UnityEngine;

namespace Game.MiniGames.Flower
{
    public class MiniGamePrefabAccumulator : MonoBehaviour
    {
        [SerializeField] private Canvas miniGameCanvas;
        [SerializeField] private PressIndicator pressIndicator;
        [SerializeField] private GameObject canPrefab;
        
        [Header("Flower Mini Game Views")]
        [SerializeField] private List<FlowerMiniGameView> flowerMiniGameViews;
        
        [Header("Park Mini Game Views")]
        [SerializeField] private List<ParkLevelView> parkLevelViews;

        public Canvas MiniGameCanvas => miniGameCanvas;
        public PressIndicator PressIndicator => pressIndicator;
        public GameObject CanPrefab => canPrefab;
        
        public List<FlowerMiniGameView> FlowerMiniGameViews => flowerMiniGameViews;
        public List<ParkLevelView> ParkLevelViews => parkLevelViews;
    }
}