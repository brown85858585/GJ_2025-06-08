using Game.MiniGames;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game.MiniGames
{
    public class FlowerMiniGameInherited : BaseTimingMiniGame
    {
        [Header("Flower Game Settings")]
        public float trackHeight = 300f;
        public float trackWidth = 60f;
        public float zoneHeight = 75f;

        [Header("Colors")]
        public Color darkRedColor = new Color(0.6f, 0f, 0f);
        public Color yellowZoneColor = Color.yellow;
        public Color greenZoneColor = Color.green;
        public Color brightRedColor = Color.red;
        public Color indicatorColor = Color.black;

        private bool isMovingUp = true;
        private float indicatorPosition = 0f;
        private float trackTop, trackBottom;
        private RectTransform trackBackground;

        // Зоны для проверки
        private RectTransform darkRedZone;
        private RectTransform yellowZone;
        private RectTransform greenZone;
        private RectTransform brightRedZone;

        protected override void CreateStartScreen()
        {
            startScreen = new GameObject("StartScreen");
            startScreen.transform.SetParent(miniGamePanel.transform, false);

            RectTransform startRect = startScreen.AddComponent<RectTransform>();
            startRect.anchorMin = Vector2.zero;
            startRect.anchorMax = Vector2.one;
            startRect.offsetMin = Vector2.zero;
            startRect.offsetMax = Vector2.zero;

            Image startBg = startScreen.AddComponent<Image>();
            startBg.color = new Color(0, 0, 0, 0.7f);

            // Заголовок и инструкции
            CreateText("Title", "🌸 Мини-игра: Полив цветка", new Vector2(0, 80), 24, Color.white, new Vector2(400, 40), startScreen.transform);
            CreateText("Instructions", "Остановите индикатор в зеленой или желтой зоне", new Vector2(0, 40), 16, Color.yellow, new Vector2(500, 30), startScreen.transform);
            CreateText("Green", "🟢 Зеленая зона = отлично", new Vector2(0, 10), 14, Color.green, new Vector2(400, 25), startScreen.transform);
            CreateText("Yellow", "🟡 Желтая зона = хорошо", new Vector2(0, -10), 14, Color.yellow, new Vector2(400, 25), startScreen.transform);
            CreateText("Red", "🔴 Красная зона = плохо", new Vector2(0, -30), 14, Color.red, new Vector2(400, 25), startScreen.transform);

            startButton = CreateButton("StartButton", "Начать полив (Пробел)", new Vector2(0, -80), new Color(0.2f, 0.8f, 0.2f), new Vector2(200, 50), startScreen.transform);
            startButton.onClick.AddListener(StartGame);

            Button startExitButton = CreateButton("StartExitButton", "Выход", new Vector2(0, -140), Color.gray, new Vector2(120, 40), startScreen.transform);
            startExitButton.onClick.AddListener(ExitMiniGame);
        }

        protected override void CreateGameScreen()
        {
            gameScreen = new GameObject("GameScreen");
            gameScreen.transform.SetParent(miniGamePanel.transform, false);
            gameScreen.SetActive(false);

            RectTransform gameRect = gameScreen.AddComponent<RectTransform>();
            gameRect.anchorMin = Vector2.zero;
            gameRect.anchorMax = Vector2.one;
            gameRect.offsetMin = Vector2.zero;
            gameRect.offsetMax = Vector2.zero;

            CreateVerticalTrack();
            CreateColorZones();
            CreateVerticalIndicator();
            CreateGameButtons();
            CreateInstructionText();
            CalculateTrackBounds();
        }

        private void CreateVerticalTrack()
        {
            // Вертикальный трек - используем Image из базового класса
            GameObject trackObj = CreateImageObject("Track", trackImage, new Vector2(trackWidth, trackHeight), Vector2.zero);

            Image currentTrackImage = trackObj.GetComponentInChildren<Image>();
            if (currentTrackImage == null)
            {
                currentTrackImage = trackObj.GetComponent<Image>();
            }

            currentTrackImage.color = Color.clear; // Прозрачный фон

            trackBackground = trackObj.GetComponent<RectTransform>();
        }

        private void CreateColorZones()
        {
            float startY = trackHeight / 2f - zoneHeight / 2f;
            Color[] zoneColors = { darkRedColor, yellowZoneColor, greenZoneColor, brightRedColor };
            string[] zoneNames = { "DarkRedZone", "YellowZone", "GreenZone", "BrightRedZone" };

            for (int i = 0; i < 4; i++)
            {
                Vector2 position = new Vector2(0, startY - zoneHeight * i);

                // Используем Image из массива zoneImages если доступен
                Image zoneImagePrefab = (zoneImages != null && i < zoneImages.Length) ? zoneImages[i] : null;

                GameObject zoneObj = CreateImageObject(zoneNames[i], zoneImagePrefab, new Vector2(trackWidth, zoneHeight), position);

                Image currentZoneImage = zoneObj.GetComponentInChildren<Image>();
                if (currentZoneImage == null)
                {
                    currentZoneImage = zoneObj.GetComponent<Image>();
                }

                currentZoneImage.color = new Color(zoneColors[i].r, zoneColors[i].g, zoneColors[i].b, 0.7f);

                RectTransform zoneRect = zoneObj.GetComponent<RectTransform>();

                // Сохранить ссылки на зоны
                switch (i)
                {
                    case 0: darkRedZone = zoneRect; break;
                    case 1: yellowZone = zoneRect; break;
                    case 2: greenZone = zoneRect; break;
                    case 3: brightRedZone = zoneRect; break;
                }
            }
        }

        private RectTransform CreateZone(string name, Color color, Vector2 position, Vector2 size)
        {
            GameObject zoneObj = new GameObject(name);
            zoneObj.transform.SetParent(gameScreen.transform, false);

            Image zoneImage = zoneObj.AddComponent<Image>();
            zoneImage.color = new Color(color.r, color.g, color.b, 0.7f);

            RectTransform zoneRect = zoneObj.GetComponent<RectTransform>();
            zoneRect.sizeDelta = size;
            zoneRect.anchoredPosition = position;

            return zoneRect;
        }

        private void CreateVerticalIndicator()
        {
            GameObject indicatorObj = new GameObject("Indicator");
            indicatorObj.transform.SetParent(gameScreen.transform, false);

            Image indicatorImage = indicatorObj.AddComponent<Image>();
            indicatorImage.color = indicatorColor;

            indicator = indicatorObj.GetComponent<RectTransform>();
            indicator.sizeDelta = new Vector2(trackWidth + 10, 15);
            indicator.anchoredPosition = new Vector2(0, -trackHeight / 2f);

            CreateArrow();
        }

        private void CreateArrow()
        {
            GameObject arrowObj = new GameObject("Arrow");
            arrowObj.transform.SetParent(indicator, false);

            Image arrowImage = arrowObj.AddComponent<Image>();
            arrowImage.color = indicatorColor;

            RectTransform arrowRect = arrowObj.GetComponent<RectTransform>();
            arrowRect.sizeDelta = new Vector2(20, 20);
            arrowRect.anchoredPosition = new Vector2(trackWidth / 2f + 15, 0);
        }

        private void CreateGameButtons()
        {
            actionButton = CreateButton("ActionButton", "Нажми E", new Vector2(-100, -trackHeight / 2f - 50), new Color(0.2f, 0.6f, 1f), new Vector2(80, 40), gameScreen.transform);
            actionButton.onClick.AddListener(OnActionButtonClick);

            exitButton = CreateButton("ExitButton", "Выход", new Vector2(100, -trackHeight / 2f - 50), Color.gray, new Vector2(80, 40), gameScreen.transform);
            exitButton.onClick.AddListener(ExitMiniGame);
        }

        private void CreateInstructionText()
        {
            instructionText = CreateText("InstructionText", "Нажми E в нужный момент!", new Vector2(trackWidth + 130, trackHeight / 2f), 16, Color.black, new Vector2(250, 40), gameScreen.transform);
        }

        private void CalculateTrackBounds()
        {
            trackBottom = -trackHeight / 2f;
            trackTop = trackHeight / 2f;
        }

        protected override void StartGameLogic()
        {
            ResetIndicator();
            StartCoroutine(MoveIndicatorVertically());
        }

        private void ResetIndicator()
        {
            if (indicator != null)
            {
                indicatorPosition = trackBottom;
                indicator.anchoredPosition = new Vector2(0, indicatorPosition);
                isMovingUp = true;
                isGameActive = true;
            }
        }

        private IEnumerator MoveIndicatorVertically()
        {
            while (isGameActive && isMovingUp)
            {
                if (indicator != null)
                {
                    indicatorPosition += indicatorSpeed * Time.deltaTime;

                    if (indicatorPosition >= trackTop)
                    {
                        indicatorPosition = trackTop;
                        isMovingUp = false;

                        yield return new WaitForSeconds(0.5f);
                        Debug.Log("Индикатор достиг верха!");
                        OnActionButtonClick(); // Автоматический "промах"
                    }

                    indicator.anchoredPosition = new Vector2(0, indicatorPosition);
                }

                yield return null;
            }
        }

        protected override void OnActionButtonClick()
        {
            if (!isGameActive)
            {
                Debug.Log("Игра неактивна, игнорируем нажатие E");
                return;
            }

            Debug.Log("✅ E обработана! Останавливаем индикатор...");

            isGameActive = false;
            isMovingUp = false;

            string result = CheckResult();

            if (result == "success")
            {
                Debug.Log("✅ Цветок полит!");
                UpdateInstructionText("🌸 Цветок полит!");
                OnGameAttempt?.Invoke(true);
                StartCoroutine(ShowResultAndEnd(1f));
            }
            else if (result == "warning")
            {
                Debug.Log("⚠️ Цветок полит, но не идеально!");
                UpdateInstructionText("🌼 Цветок полит!");
                OnGameAttempt?.Invoke(true);
                StartCoroutine(ShowResultAndEnd(1f));
            }
            else
            {
                Debug.Log($"❌ Сейчас завянет!");
                UpdateInstructionText("💀 Сейчас завянет!");
                OnGameAttempt?.Invoke(false);
                StartCoroutine(ShowResultAndEnd(1.5f));
            }
        }

        protected override string CheckResult()
        {
            if (indicator == null) return "fail";

            float indicatorY = indicator.anchoredPosition.y;

            // Проверить зеленую зону (лучший результат)
            if (IsInZone(indicatorY, greenZone))
            {
                return "success";
            }

            // Проверить желтую зону (хороший результат)
            if (IsInZone(indicatorY, yellowZone))
            {
                return "warning";
            }

            // Проверить красные зоны (плохой результат)
            if (IsInZone(indicatorY, darkRedZone) || IsInZone(indicatorY, brightRedZone))
            {
                return "fail";
            }

            return "fail";
        }

        private bool IsInZone(float indicatorY, RectTransform zone)
        {
            if (zone == null) return false;

            float zoneBottom = zone.anchoredPosition.y - zoneHeight / 2f;
            float zoneTop = zone.anchoredPosition.y + zoneHeight / 2f;

            return indicatorY >= zoneBottom && indicatorY <= zoneTop;
        }

        // Публичные методы для интеграции
        public void SetDifficulty(float speed, float zones)
        {
            indicatorSpeed = speed;
            zoneHeight = zones;
        }

        public void StartWatering()
        {
            StartMiniGame();
        }
    }
}