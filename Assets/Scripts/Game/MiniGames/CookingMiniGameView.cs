using UnityEngine;

namespace Game.MiniGames
{
    public class CookingMiniGameView : MonoBehaviour
    {
        [Header("")]
        [SerializeField] public GameObject CookingViewPrefab;
        [SerializeField] public int gameSpeed; 
        [SerializeField] public GameObject winZone; 
        [SerializeField] public GameObject knife;
    }
}