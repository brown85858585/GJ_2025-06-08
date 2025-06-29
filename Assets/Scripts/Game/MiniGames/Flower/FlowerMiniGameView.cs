using UnityEngine;

namespace Game.MiniGames.Flower
{
    public class FlowerMiniGameView : MonoBehaviour
    {
        [SerializeField] private Transform pressPoint;
        [SerializeField] private int winScore;
        
        public Transform PressPoint => pressPoint;
        public int WinScore => winScore;
    }
}