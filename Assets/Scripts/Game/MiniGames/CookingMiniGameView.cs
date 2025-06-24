using UnityEngine;
using UnityEngine.UI;

namespace Game.MiniGames
{
    public class CookingMiniGameView : MonoBehaviour
    {
        [Header("")]
        [SerializeField] public GameObject CookingViewPrefab;
        [SerializeField] public int gameSpeed; 
        [SerializeField] public GameObject winZone; 
        [SerializeField] public GameObject knife;
        [SerializeField] public float winZoneWidth = 300;

        /*
        public CookingViewComponents cookingComponents;
        [System.Serializable]
        public class CookingViewComponents
        {
            [Header("Handlers")]
            public Transform knifeHandler;
            public Transform winZoneHandler;

            [Header("Interactive Elements")]
            public RectTransform knife;
            public Image winZone;

            [Header("Background Elements")]
            public Transform backgroundLayer;
            public Image[] vegetables; // Массив изображений овощей

            [Header("Frames")]
            public Image backFrame;
            public Image topFrame;

            public bool IsValid()
            {
                return knifeHandler != null && winZoneHandler != null &&
                       knife != null && winZone != null;
            }
        }
        */
    }
}