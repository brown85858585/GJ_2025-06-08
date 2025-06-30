using UnityEngine;

namespace Game.MiniGames.Flower
{
    public class FlowerMiniGameView : MonoBehaviour
    {
        [SerializeField] private RectTransform pressPoint;
        [SerializeField] private RectTransform canPoint;
        [SerializeField] private RectTransform canDefaultTransform;
        [SerializeField] private int winScore = 500;
        [SerializeField] private float pressForce = 0.1f;
        
        public Transform PressPoint => pressPoint;
        public Transform CanPoint => canPoint;
        public RectTransform CanDefaultTransform => canDefaultTransform;
        public int WinScore => winScore;
        public float PressForce => pressForce;
    }
}