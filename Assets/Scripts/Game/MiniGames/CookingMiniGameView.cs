using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace Game.MiniGames
{
    public class CookingMiniGameView : MonoBehaviour
    {
        [Header("")]
        [SerializeField] public GameObject CookingViewPrefab;
        [SerializeField] private CookingMiniGame cookingMiniGame;
        [SerializeField] public int gameSpeed;

        [SerializeField] private UIElementTweener board;
        [SerializeField] private UIElementTweener knife;
        [SerializeField] private UIElementTweener actionButton;
        [SerializeField] private UIElementTweener firstItem;
        [SerializeField] private UIElementTweener secondItem;
        [SerializeField] private UIElementTweener thirdItem;

        int dotweenKnifeAndActionButtonCount = 0; 

        private void OnEnable()
        {
            board.OnShowComplete += Board_OnShowComplete;
            board.gameObject.SetActive(true);
            board.Show();
        }

        private void OnDisable()
        {
            board.Hide();
            //knife.Hide();
            actionButton.Hide();
            firstItem.Hide();
            secondItem.Hide();
            thirdItem.Hide();
        }

        private void OnDestroy()
        {
            board.OnShowComplete -= Board_OnShowComplete;
            knife.OnShowComplete -= Knife_OnShowComplete;
            actionButton.OnShowComplete -= ActionButton_OnShowComplete;
            firstItem.OnShowComplete -= FirstItem_OnShowComplete;
            secondItem.OnShowComplete -= SecondItem_OnShowComplete;
            thirdItem.OnShowComplete -= ThirdItem_OnShowComplete;
        }

        private void Board_OnShowComplete()
        {
            knife.OnShowComplete += Knife_OnShowComplete;
            knife.gameObject.SetActive(true);
            knife.Show();
            actionButton.OnShowComplete += ActionButton_OnShowComplete;
            actionButton.gameObject.SetActive(true);
            actionButton.enabled = false;
            actionButton.Show();
        }

        private void Knife_OnShowComplete()
        {
            CompleteKnifeAndActionButtonAnimations();
        }

        private void ActionButton_OnShowComplete()
        {
            CompleteKnifeAndActionButtonAnimations();
        }

        private void CompleteKnifeAndActionButtonAnimations()
        {
            dotweenKnifeAndActionButtonCount++;

            if (dotweenKnifeAndActionButtonCount > 0)
            {
                firstItem.OnShowComplete += FirstItem_OnShowComplete;
                firstItem.gameObject.SetActive(true);
                firstItem.Show();
            }
        }

        private void FirstItem_OnShowComplete()
        {
            secondItem.OnShowComplete += SecondItem_OnShowComplete;
            secondItem.gameObject.SetActive(true);
            secondItem.Show();
        }

        private void SecondItem_OnShowComplete()
        {
            thirdItem.OnShowComplete += ThirdItem_OnShowComplete;
            thirdItem.gameObject.SetActive(true);
            thirdItem.Show();
        }

        private void ThirdItem_OnShowComplete()
        {
            // Start gameplay
        }

        ////[SerializeField] public GameObject winZone; 
        //[SerializeField] public GameObject knife;
        //[SerializeField] public float winZoneWidth = 300;

        //[Header("Win Zones - Multiple")]
        //[SerializeField] public GameObject[] winZones = new GameObject[3]; // Ссылки на все 3 winZone
        //[SerializeField] public float[] winZoneWidths = { 80f, 70f, 60f }; // Размеры для каждой зоны

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