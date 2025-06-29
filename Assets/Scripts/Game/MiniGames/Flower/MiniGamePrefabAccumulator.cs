using System.Collections.Generic;
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

        public Canvas MiniGameCanvas => miniGameCanvas;
        public PressIndicator PressIndicator => pressIndicator;
        public GameObject CanPrefab => canPrefab;
        
        public List<FlowerMiniGameView> FlowerMiniGameViews => flowerMiniGameViews;
    }
}