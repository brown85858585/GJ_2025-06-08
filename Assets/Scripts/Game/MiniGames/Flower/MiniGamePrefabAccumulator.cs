using System.Collections.Generic;
using UI.Flower;
using UnityEngine;

namespace Game.MiniGames.Flower
{
    public class MiniGamePrefabAccumulator : MonoBehaviour
    {
        [SerializeField] private Canvas miniGameCanvas;
        [SerializeField] private PressIndicator pressIndicator;
        [SerializeField] private List<FlowerMiniGameView> flowerMiniGameViews;

        public Canvas MiniGameCanvas => miniGameCanvas;
        public PressIndicator PressIndicator => pressIndicator;
        public List<FlowerMiniGameView> FlowerMiniGameViews => flowerMiniGameViews;
    }
}