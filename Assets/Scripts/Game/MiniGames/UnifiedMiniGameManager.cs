using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace Game.MiniGames
{
    public enum MiniGameType
    {
        Flower,
        Cooking
    }

    public class UnifiedMiniGameManager : BaseTimingMiniGame
    {
        [Header("Game Type")]
        [SerializeField] private MiniGameType gameType = MiniGameType.Flower;



        public override void SetupInput()
        {
            Debug.Log("🎮 Настройка Input для UnifiedMiniGameManager");

            // Если Input Actions назначены в Inspector - используем базовую логику
            if (actionInputAction != null && startInputAction != null)
            {
                base.SetupInput();
                Debug.Log("✅ Используем Input Actions из Inspector");
                return;
            }

            // Иначе просто предупреждение - input будет через coordinator
            Debug.LogWarning("⚠️ Input Actions не назначены. Input будет обрабатываться через MiniGameCoordinator");
        }

        private void SetupAlternativeInput()
        {
            Debug.Log("🔧 Input будет настроен через MiniGameCoordinator");
            // Coordinator сам настроит input через _playerController
        }

        private void OnDestroy()
        {
            // Очистка при уничтожении объекта
            Debug.Log("🗑️ UnifiedMiniGameManager уничтожен");
        }

        private void AutoFindComponents()
        {
            // Автопоиск Canvas если не назначен
            if (mainCanvas == null)
            {
                mainCanvas = FindObjectOfType<Canvas>();
                if (mainCanvas != null)
                {
                    Debug.Log($"✅ Canvas найден автоматически: {mainCanvas.name}");
                }
            }

            // Найти или создать Panel
            if (panel == null)
            {
                var foundPanel = GameObject.Find("MiniGamePanel");
                if (foundPanel != null)
                {
                    panel = foundPanel;
                    Debug.Log($"✅ Panel найден автоматически: {foundPanel.name}");
                }
                else if (mainCanvas != null)
                {
                    // СОЗДАТЬ панель программно если не найдена
                    panel = CreateMiniGamePanel();
                    Debug.Log($"🔧 Panel создан автоматически: {panel.name}");
                }
                else
                {
                    Debug.LogError("❌ Невозможно создать панель - Canvas не найден!");
                }
            }

            // Проверка результатов
            if (mainCanvas == null)
            {
                Debug.LogError("❌ Main Canvas не найден! Добавь Canvas в сцену");
            }

            if (panel == null)
            {
                Debug.LogError("❌ Panel не найден и не создан!");
            }
            else
            {
                Debug.Log($"✅ Всё готово: Canvas={mainCanvas.name}, Panel={panel.name}");
            }
        }

        private GameObject CreateMiniGamePanel()
        {
            GameObject newPanel = new GameObject("MiniGamePanel");
            newPanel.transform.SetParent(mainCanvas.transform, false);

            // Добавить Image компонент для фона
            Image panelImage = newPanel.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.8f); // Полупрозрачный черный

            // Настроить RectTransform на весь экран
            RectTransform panelRect = newPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // По умолчанию выключить
            newPanel.SetActive(false);

            return newPanel;
        }

        // Логика для разных игр
        private IGameLogic currentGameLogic;
        private FlowerGameLogic flowerLogic;
        private CookingGameLogic cookingLogic;

        protected override void Start()
        {
            // Инициализировать логику игр
            flowerLogic = new FlowerGameLogic(this);
            cookingLogic = new CookingGameLogic(this);

            base.Start();
        }

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

            // Выбрать логику по типу игры
            switch (gameType)
            {
                case MiniGameType.Flower:
                    currentGameLogic = flowerLogic;
                    break;
                case MiniGameType.Cooking:
                    currentGameLogic = cookingLogic;
                    break;
            }

            // Создать стартовый экран через выбранную логику
            currentGameLogic.CreateStartScreen(startScreen.transform);

            // Общие кнопки
            startButton = CreateButton("StartButton", $"Начать {GetGameName()} (Пробел)",
                new Vector2(0, -130), new Color(0.2f, 0.8f, 0.2f), new Vector2(220, 50), startScreen.transform);
            startButton.onClick.AddListener(StartGame);

            Button startExitButton = CreateButton("StartExitButton", "Выход",
                new Vector2(0, -190), Color.gray, new Vector2(120, 40), startScreen.transform);
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

            // Создать игровой экран через выбранную логику
            currentGameLogic.CreateGameScreen(gameScreen.transform);

            // Общие кнопки
            actionButton = CreateButton("ActionButton", "Действие (E)",
                new Vector2(-100, -200), new Color(0.2f, 0.6f, 1f), new Vector2(120, 40), gameScreen.transform);
            actionButton.onClick.AddListener(OnActionButtonClick);

            exitButton = CreateButton("ExitButton", "Выход",
                new Vector2(100, -200), Color.gray, new Vector2(80, 40), gameScreen.transform);
            exitButton.onClick.AddListener(ExitMiniGame);

            instructionText = CreateText("InstructionText", currentGameLogic.GetInstructionText(),
                new Vector2(0, 200), 16, Color.black, new Vector2(300, 40), gameScreen.transform);
        }

        protected override void StartGameLogic()
        {
            currentGameLogic.StartGame();
        }

        public override void OnActionButtonClick()
        {
            if (!isGameActive) return;

            isGameActive = false;
            string result = currentGameLogic.OnAction();

            if (result == "success")
            {
                UpdateInstructionText(currentGameLogic.GetSuccessMessage());
                OnGameAttempt?.Invoke(true);
                StartCoroutine(ShowResultAndEnd(1.5f));
            }
            else
            {
                UpdateInstructionText(currentGameLogic.GetFailMessage());
                OnGameAttempt?.Invoke(false);
                StartCoroutine(ShowResultAndEnd(1.5f));
            }
        }

        protected override string CheckResult()
        {
            return currentGameLogic.CheckResult();
        }

        private string GetGameName()
        {
            return gameType switch
            {
                MiniGameType.Flower => "полив цветка",
                MiniGameType.Cooking => "готовку еды",
                _ => "игру"
            };
        }

        // Публичные методы для установки типа игры
        public void SetGameType(MiniGameType type)
        {
            gameType = type;
        }

        public void StartFlowerGame()
        {
            SetGameType(MiniGameType.Flower);
            StartMiniGame();
        }

        public void StartCookingGame()
        {
            SetGameType(MiniGameType.Cooking);
            StartMiniGame();
        }
    }

    // Интерфейс для логики игр
    public interface IGameLogic
    {
        void CreateStartScreen(Transform parent);
        void CreateGameScreen(Transform parent);
        void StartGame();
        string OnAction();
        string CheckResult();
        string GetInstructionText();
        string GetSuccessMessage();
        string GetFailMessage();
    }

    // Логика игры полива цветов
    public class FlowerGameLogic : IGameLogic
    {
        private UnifiedMiniGameManager manager;
        private RectTransform indicator;
        private RectTransform[] zones = new RectTransform[4];
        private float indicatorPosition;
        private bool isMovingUp = true;

        // Настройки
        private float trackHeight = 300f;
        private float trackWidth = 60f;
        private float zoneHeight = 75f;

        public FlowerGameLogic(UnifiedMiniGameManager manager)
        {
            this.manager = manager;
        }

        public void CreateStartScreen(Transform parent)
        {
            manager.CreateText("Title", "🌸 Мини-игра: Полив цветка", new Vector2(0, 80), 24, Color.white, new Vector2(400, 40), parent);
            manager.CreateText("Instructions", "Остановите индикатор в зеленой или желтой зоне", new Vector2(0, 40), 16, Color.yellow, new Vector2(500, 30), parent);
            manager.CreateText("Green", "🟢 Зеленая зона = отлично", new Vector2(0, 10), 14, Color.green, new Vector2(400, 25), parent);
            manager.CreateText("Yellow", "🟡 Желтая зона = хорошо", new Vector2(0, -10), 14, Color.yellow, new Vector2(400, 25), parent);
            manager.CreateText("Red", "🔴 Красная зона = плохо", new Vector2(0, -30), 14, Color.red, new Vector2(400, 25), parent);
        }

        public void CreateGameScreen(Transform parent)
        {
            // Создать вертикальный трек и зоны
            CreateVerticalTrack(parent);
            CreateColorZones(parent);
            CreateIndicator(parent);
        }

        private void CreateVerticalTrack(Transform parent)
        {
            GameObject trackObj = new GameObject("Track");
            trackObj.transform.SetParent(parent, false);

            Image trackImage = trackObj.AddComponent<Image>();
            trackImage.color = Color.clear;

            RectTransform trackRect = trackObj.GetComponent<RectTransform>();
            trackRect.sizeDelta = new Vector2(trackWidth, trackHeight);
            trackRect.anchoredPosition = Vector2.zero;
        }

        private void CreateColorZones(Transform parent)
        {
            float startY = trackHeight / 2f - zoneHeight / 2f;
            Color[] colors = { new Color(0.6f, 0f, 0f), Color.yellow, Color.green, Color.red };
            string[] names = { "DarkRed", "Yellow", "Green", "BrightRed" };

            for (int i = 0; i < 4; i++)
            {
                GameObject zoneObj = new GameObject(names[i] + "Zone");
                zoneObj.transform.SetParent(parent, false);

                Image zoneImage = zoneObj.AddComponent<Image>();
                zoneImage.color = new Color(colors[i].r, colors[i].g, colors[i].b, 0.7f);

                RectTransform zoneRect = zoneObj.GetComponent<RectTransform>();
                zoneRect.sizeDelta = new Vector2(trackWidth, zoneHeight);
                zoneRect.anchoredPosition = new Vector2(0, startY - zoneHeight * i);

                zones[i] = zoneRect;
            }
        }

        private void CreateIndicator(Transform parent)
        {
            GameObject indicatorObj = new GameObject("Indicator");
            indicatorObj.transform.SetParent(parent, false);

            Image indicatorImage = indicatorObj.AddComponent<Image>();
            indicatorImage.color = Color.black;

            indicator = indicatorObj.GetComponent<RectTransform>();
            indicator.sizeDelta = new Vector2(trackWidth + 10, 15);
            indicator.anchoredPosition = new Vector2(0, -trackHeight / 2f);
        }

        public void StartGame()
        {
            indicatorPosition = -trackHeight / 2f;
            isMovingUp = true;
            manager.StartCoroutine(MoveIndicator());
        }

        private IEnumerator MoveIndicator()
        {
            while (manager.isGameActive && isMovingUp)
            {
                indicatorPosition += manager.indicatorSpeed * Time.deltaTime;

                if (indicatorPosition >= trackHeight / 2f)
                {
                    indicatorPosition = trackHeight / 2f;
                    isMovingUp = false;
                    yield return new WaitForSeconds(0.5f);
                    manager.OnActionButtonClick(); // Автопромах
                }

                indicator.anchoredPosition = new Vector2(0, indicatorPosition);
                yield return null;
            }
        }

        public string OnAction()
        {
            isMovingUp = false;
            return CheckResult();
        }

        public string CheckResult()
        {
            // Проверка попадания в зоны
            if (IsInZone(indicatorPosition, zones[2])) return "success"; // Зеленая
            if (IsInZone(indicatorPosition, zones[1])) return "warning"; // Желтая
            return "fail"; // Красные зоны
        }

        private bool IsInZone(float pos, RectTransform zone)
        {
            if (zone == null) return false;
            float bottom = zone.anchoredPosition.y - zoneHeight / 2f;
            float top = zone.anchoredPosition.y + zoneHeight / 2f;
            return pos >= bottom && pos <= top;
        }

        public string GetInstructionText() => "Нажми E в нужный момент!";
        public string GetSuccessMessage() => "🌸 Цветок полит!";
        public string GetFailMessage() => "💀 Сейчас завянет!";
    }

    // Логика игры готовки
    public class CookingGameLogic : IGameLogic
    {
        private UnifiedMiniGameManager manager;
        private RectTransform indicator;
        private float currentAngle;
        private float targetAngle;
        private bool movingClockwise = true;

        // Настройки
        private float arcRadius = 150f;
        private float arcStartAngle = 200f;
        private float arcEndAngle = 340f;
        private float successZoneAngle = 45f;

        public CookingGameLogic(UnifiedMiniGameManager manager)
        {
            this.manager = manager;
        }

        public void CreateStartScreen(Transform parent)
        {
            manager.CreateText("Title", "🍳 Готовка еды", new Vector2(0, 100), 24, Color.white, new Vector2(400, 40), parent);
            manager.CreateText("Subtitle", "Алгоритм:", new Vector2(0, 60), 18, Color.yellow, new Vector2(400, 30), parent);
            manager.CreateText("Step1", "1. Игрок нажимает на кухню", new Vector2(0, 20), 14, Color.white, new Vector2(400, 25), parent);
            manager.CreateText("Step2", "2. ГГ подходит к холодильнику, берет пакет", new Vector2(0, -5), 14, Color.white, new Vector2(400, 25), parent);
            manager.CreateText("Step3", "3. Запускается анимация нарезки", new Vector2(0, -30), 14, Color.white, new Vector2(400, 25), parent);
            manager.CreateText("Step4", "4. Мини-игра: остановите стрелку в зеленой зоне", new Vector2(0, -55), 14, Color.green, new Vector2(400, 25), parent);
            manager.CreateText("Step5", "5. После завершения ГГ садится за стол и ест", new Vector2(0, -80), 14, Color.white, new Vector2(400, 25), parent);
        }

        public void CreateGameScreen(Transform parent)
        {
            CreateArc(parent);
            CreateSuccessZone(parent);
            CreateIndicator(parent);
        }

        private void CreateArc(Transform parent)
        {
            GameObject arcObj = new GameObject("Arc");
            arcObj.transform.SetParent(parent, false);

            Image arcImage = arcObj.AddComponent<Image>();
            arcImage.color = Color.gray;

            RectTransform arcRect = arcObj.GetComponent<RectTransform>();
            arcRect.sizeDelta = new Vector2(arcRadius * 2, arcRadius * 2);
            arcRect.anchoredPosition = Vector2.zero;
        }

        private void CreateSuccessZone(Transform parent)
        {
            GameObject successObj = new GameObject("SuccessZone");
            successObj.transform.SetParent(parent, false);

            Image successImage = successObj.AddComponent<Image>();
            successImage.color = Color.green;

            RectTransform successRect = successObj.GetComponent<RectTransform>();
            successRect.sizeDelta = new Vector2(arcRadius * 2, arcRadius * 2);
            successRect.anchoredPosition = Vector2.zero;

            // Случайная позиция зеленой зоны
            targetAngle = Random.Range(arcStartAngle + successZoneAngle / 2, arcEndAngle - successZoneAngle / 2);
        }

        private void CreateIndicator(Transform parent)
        {
            GameObject indicatorObj = new GameObject("Indicator");
            indicatorObj.transform.SetParent(parent, false);

            Image indicatorImage = indicatorObj.AddComponent<Image>();
            indicatorImage.color = Color.black;

            indicator = indicatorObj.GetComponent<RectTransform>();
            indicator.sizeDelta = new Vector2(10, arcRadius + 20);
            indicator.anchoredPosition = Vector2.zero;
            indicator.pivot = new Vector2(0.5f, 0f);
        }

        public void StartGame()
        {
            currentAngle = arcStartAngle;
            movingClockwise = true;
            manager.StartCoroutine(MoveIndicator());
        }

        private IEnumerator MoveIndicator()
        {
            while (manager.isGameActive)
            {
                float angleSpeed = manager.indicatorSpeed * Time.deltaTime;

                if (movingClockwise)
                {
                    currentAngle += angleSpeed;
                    if (currentAngle >= arcEndAngle)
                    {
                        currentAngle = arcEndAngle;
                        movingClockwise = false;
                    }
                }
                else
                {
                    currentAngle -= angleSpeed;
                    if (currentAngle <= arcStartAngle)
                    {
                        currentAngle = arcStartAngle;
                        movingClockwise = true;
                    }
                }

                indicator.rotation = Quaternion.Euler(0, 0, -currentAngle);
                yield return null;
            }
        }

        public string OnAction()
        {
            return CheckResult();
        }

        public string CheckResult()
        {
            float zoneStart = targetAngle - successZoneAngle / 2f;
            float zoneEnd = targetAngle + successZoneAngle / 2f;

            if (currentAngle >= zoneStart && currentAngle <= zoneEnd)
                return "success";

            return "fail";
        }

        public string GetInstructionText() => "Остановите стрелку в зеленой зоне!";
        public string GetSuccessMessage() => "🍽️ Идеально приготовлено!";
        public string GetFailMessage() => "🔥 Еда подгорела!";
    }
}