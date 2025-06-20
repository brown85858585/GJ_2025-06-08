using System.Collections;
using Game.MiniGames;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks.Triggers;

namespace Game.MiniGames
{

    public class FlowerMiniGameManager : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private Canvas mainCanvas;
        [SerializeField] private GameObject panel;

        public GameObject Panel => panel;

        private GameObject miniGamePanel;

        [Header("Prefab References")]
        [SerializeField] private GameObject flowerGameViewPrefab; // Префаб FlowerGameView
        private GameObject instantiatedFlowerView;

        // Ссылки на элементы из префаба
        private RectTransform waterMask; // Маска для воды
        private Image waterImage; // Изображение воды
        private RectTransform flowerContainer; // Контейнер цветка

        [Header("UI Elements")]
        private Button actionButton;
        private Button exitButton;
        private Button startButton;
        private Button startExitButton;
        private Text instructionText;
        private GameObject startScreen;
        private GameObject gameScreen;

        [Header("Game Settings")]
        public float indicatorSpeed = 100f;
        public float trackHeight = 300f; // Высота зоны для воды
        public float zoneHeight = 75f;
        public int maxAttempts = 3;

        [Header("Water Animation")]
        [SerializeField] private float waterMinY = -200f; // Минимальная позиция воды
        [SerializeField] private float waterMaxY = 200f;  // Максимальная позиция воды

        private bool isGameActive = false;
        private bool isMovingUp = true;
        private int currentAttempts = 0;
        private float waterPosition = 0f; // Позиция воды вместо индикатора
        private bool gameStarted = false;

        // Зоны для проверки (в координатах воды)
        private float darkRedZoneMin, darkRedZoneMax;
        private float yellowZoneMin, yellowZoneMax;
        private float greenZoneMin, greenZoneMax;
        private float brightRedZoneMin, brightRedZoneMax;

        [Header("Input")]
        [SerializeField] private InputActionReference actionInputAction;
        [SerializeField] private InputActionReference startInputAction;
        private bool useDirectInput = false;

        // События
        public System.Action OnMiniGameComplete;
        public System.Action<bool> OnWateringAttempt;

        void Start()
        {
            FindSceneComponents();
            SetupInput();

            if (miniGamePanel != null)
            {
                miniGamePanel.SetActive(false);
            }

            CreateMiniGameUI();
            SetupWaterZones();
        }

        private void SetupInput()
        {
            if (actionInputAction == null || startInputAction == null)
            {
                useDirectInput = true;
                Debug.Log("InputActionReference не назначены, используем прямой ввод клавиш");
                return;
            }

            actionInputAction.action.performed += OnActionInput;
            startInputAction.action.performed += OnStartInput;
        }

        private void OnDestroy()
        {
            if (actionInputAction != null)
                actionInputAction.action.performed -= OnActionInput;
            if (startInputAction != null)
                startInputAction.action.performed -= OnStartInput;
        }

        private void OnActionInput(InputAction.CallbackContext context)
        {
            if (gameStarted && isGameActive)
            {
                OnActionButtonClick();
            }
        }

        private void OnStartInput(InputAction.CallbackContext context)
        {
            if (!gameStarted)
            {
                StartGame();
            }
        }

        private void FindSceneComponents()
        {
            if (mainCanvas == null)
            {
                Debug.LogError("Canvas не найден в сцене!");
                return;
            }

            miniGamePanel = GameObject.Find("MiniGamePanel");
            if (miniGamePanel == null)
            {
                Debug.LogError("MiniGamePanel не найдена в Canvas!");
                return;
            }
        }

        private void CreateMiniGameUI()
        {
            if (miniGamePanel == null) return;

            miniGamePanel.SetActive(false);
            ClearExistingElements();

            // Создать стартовый экран
            CreateStartScreen();

            // Создать игровой экран с префабом
            CreateGameScreenWithPrefab();
        }

        private void CreateGameScreenWithPrefab()
        {
            // Контейнер для игрового экрана
            gameScreen = new GameObject("GameScreen");
            gameScreen.transform.SetParent(miniGamePanel.transform, false);
            gameScreen.SetActive(false);

            RectTransform gameRect = gameScreen.AddComponent<RectTransform>();
            gameRect.anchorMin = Vector2.zero;
            gameRect.anchorMax = Vector2.one;
            gameRect.offsetMin = Vector2.zero;
            gameRect.offsetMax = Vector2.zero;

            // Инстанцировать префаб цветка
            if (flowerGameViewPrefab != null)
            {
                instantiatedFlowerView = Instantiate(flowerGameViewPrefab, gameScreen.transform);

                // Настроить RectTransform для префаба
                RectTransform flowerRect = instantiatedFlowerView.GetComponent<RectTransform>();
                if (flowerRect != null)
                {
                    flowerRect.anchorMin = Vector2.zero;
                    flowerRect.anchorMax = Vector2.one;
                    flowerRect.offsetMin = Vector2.zero;
                    flowerRect.offsetMax = Vector2.zero;
                    flowerRect.localScale = new Vector3(1f, 1f, 1f); // Устанавливаем scale 0.5
                }

                // Найти компоненты в префабе
                FindPrefabComponents();
            }
            else
            {
                Debug.LogError("FlowerGameView prefab не назначен!");
            }

            CreateGameButtons();
            CreateInstructionText();
        }

        private void FindPrefabComponents()
        {
            if (instantiatedFlowerView == null) return;

            // Найти маску и изображение воды
            Transform maskTransform = instantiatedFlowerView.transform.Find("Panel/Mask");
            if (maskTransform != null)
            {
                waterMask = maskTransform.GetComponent<RectTransform>();

                Transform waterTransform = maskTransform.Find("Water");
                if (waterTransform != null)
                {
                    waterImage = waterTransform.GetComponent<Image>();
                }
            }

            // Найти контейнер цветка (если нужно)
            Transform flowerTransform = instantiatedFlowerView.transform.Find("Panel");
            if (flowerTransform != null)
            {
                flowerContainer = flowerTransform.GetComponent<RectTransform>();
            }

            if (waterMask == null || waterImage == null)
            {
                Debug.LogError("Не удалось найти компоненты воды в префабе!");
            }
        }

        private void SetupWaterZones()
        {
            // Определить зоны в координатах воды
            float zoneSize = (waterMaxY - waterMinY) / 4f;

            brightRedZoneMin = waterMinY;
            brightRedZoneMax = waterMinY + zoneSize;

            greenZoneMin = waterMinY + zoneSize;
            greenZoneMax = waterMinY + zoneSize * 2;

            yellowZoneMin = waterMinY + zoneSize * 2;
            yellowZoneMax = waterMinY + zoneSize * 3;

            darkRedZoneMin = waterMinY + zoneSize * 3;
            darkRedZoneMax = waterMaxY;
        }

        private void CreateStartScreen()
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

            CreateStartText("Мини-игра: Полив цветка", new Vector2(0, 80), 24, Color.white);
            CreateStartText("Остановите воду в нужной зоне", new Vector2(0, 40), 16, Color.yellow);
            CreateStartText("🟢 Зеленая зона = отлично", new Vector2(0, 10), 14, Color.green);
            CreateStartText("🟡 Желтая зона = хорошо", new Vector2(0, -10), 14, Color.yellow);
            CreateStartText("🔴 Красная зона = плохо", new Vector2(0, -30), 14, Color.red);

            startButton = CreateStartButton("StartButton", "Начать игру (Пробел)", new Vector2(0, -80), new Color(0.2f, 0.8f, 0.2f), new Vector2(200, 50));
            startButton.onClick.AddListener(StartGame);

            startExitButton = CreateStartButton("StartExitButton", "Выход", new Vector2(0, -140), Color.gray, new Vector2(120, 40));
            startExitButton.onClick.AddListener(ExitMiniGame);
        }

        private void CreateStartText(string text, Vector2 position, int fontSize, Color color)
        {
            GameObject textObj = new GameObject("StartText");
            textObj.transform.SetParent(startScreen.transform, false);

            Text startText = textObj.AddComponent<Text>();
            startText.text = text;
            startText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            startText.alignment = TextAnchor.MiddleCenter;
            startText.color = color;
            startText.fontSize = fontSize;

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(400, 30);
            textRect.anchoredPosition = position;
        }

        private Button CreateStartButton(string name, string text, Vector2 position, Color color, Vector2 size)
        {
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(startScreen.transform, false);

            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = color;

            Button button = buttonObj.AddComponent<Button>();

            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.sizeDelta = size;
            buttonRect.anchoredPosition = position;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);

            Text buttonText = textObj.AddComponent<Text>();
            buttonText.text = text;
            buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.color = Color.white;
            buttonText.fontSize = 12;

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return button;
        }

        private void CreateGameButtons()
        {
            actionButton = CreateButton("ActionButton", "Нажми E", new Vector2(-100, -250), new Color(0.2f, 0.6f, 1f), new Vector2(80, 40));
            actionButton.onClick.AddListener(OnActionButtonClick);
            actionButton.transform.SetParent(gameScreen.transform, false);

            exitButton = CreateButton("ExitButton", "Выход", new Vector2(100, -250), Color.gray, new Vector2(80, 40));
            exitButton.onClick.AddListener(ExitMiniGame);
            exitButton.transform.SetParent(gameScreen.transform, false);
        }

        private Button CreateButton(string name, string text, Vector2 position, Color color, Vector2 size)
        {
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(gameScreen.transform, false);

            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = color;

            Button button = buttonObj.AddComponent<Button>();

            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.sizeDelta = size;
            buttonRect.anchoredPosition = position;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);

            Text buttonText = textObj.AddComponent<Text>();
            buttonText.text = text;
            buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.color = Color.white;
            buttonText.fontSize = 12;

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return button;
        }

        private void CreateInstructionText()
        {
            GameObject textObj = new GameObject("InstructionText");
            textObj.transform.SetParent(gameScreen.transform, false);

            instructionText = textObj.AddComponent<Text>();
            instructionText.text = "Нажми E в нужный момент!";
            instructionText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            instructionText.alignment = TextAnchor.MiddleCenter;
            instructionText.color = Color.black;
            instructionText.fontSize = 16;

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(250, 40);
            textRect.anchoredPosition = new Vector2(0, 250);
        }

        private void ClearExistingElements()
        {
            Transform[] children = miniGamePanel.GetComponentsInChildren<Transform>();
            for (int i = children.Length - 1; i >= 0; i--)
            {
                if (children[i] != miniGamePanel.transform)
                {
                    DestroyImmediate(children[i].gameObject);
                }
            }
        }

        // ПУБЛИЧНЫЕ МЕТОДЫ

        public void StartMiniGame()
        {
            if (miniGamePanel == null)
            {
                Debug.LogError("Мини-игра не инициализирована!");
                return;
            }

            if (!useDirectInput)
            {
                actionInputAction?.action.Enable();
                startInputAction?.action.Enable();
            }

            miniGamePanel.SetActive(true);

            if (startScreen != null)
            {
                startScreen.SetActive(true);
            }

            if (gameScreen != null)
            {
                gameScreen.SetActive(false);
            }

            gameStarted = false;
            isGameActive = false;
        }

        private void StartGame()
        {
            if (startScreen != null)
            {
                startScreen.SetActive(false);
            }

            if (gameScreen != null)
            {
                gameScreen.SetActive(true);
            }

            gameStarted = true;
            currentAttempts = 0;
            isGameActive = true;

            ResetWater();
            StartCoroutine(MoveWater());
        }

        public void EndMiniGame()
        {
            isGameActive = false;
            gameStarted = false;

            if (!useDirectInput)
            {
                actionInputAction?.action.Disable();
                startInputAction?.action.Disable();
            }

            if (miniGamePanel != null)
            {
                miniGamePanel.SetActive(false);
            }

            OnMiniGameComplete?.Invoke();
        }

        private void ExitMiniGame()
        {
            EndMiniGame();
        }

        // ЛОГИКА ИГРЫ С ВОДОЙ

        private void ResetWater()
        {
            if (waterImage != null)
            {
                waterPosition = waterMinY;
                UpdateWaterPosition();
                isMovingUp = true;
                isGameActive = true;
            }
        }

        private void UpdateWaterPosition()
        {
            if (waterImage != null)
            {
                RectTransform waterRect = waterImage.GetComponent<RectTransform>();
                waterRect.anchoredPosition = new Vector2(waterRect.anchoredPosition.x, waterPosition);
            }
        }

        private IEnumerator MoveWater()
        {
            while (isGameActive && isMovingUp)
            {
                if (waterImage != null)
                {
                    waterPosition += indicatorSpeed * Time.deltaTime;

                    if (waterPosition >= waterMaxY)
                    {
                        waterPosition = waterMaxY;
                        isMovingUp = false;

                        yield return new WaitForSeconds(0.5f);
                        OnActionButtonClick();
                    }

                    UpdateWaterPosition();
                }

                yield return null;
            }
        }

        private void OnActionButtonClick()
        {
            if (!isGameActive)
            {
                return;
            }

            isGameActive = false;
            isMovingUp = false;

            string result = CheckWaterZone();

            if (result == "success")
            {
                OnWateringAttempt?.Invoke(true);
                StartCoroutine(ShowResultAndEnd(1f));
            }
            else if (result == "warning")
            {
                OnWateringAttempt?.Invoke(true);
                StartCoroutine(ShowResultAndEnd(1f));
            }
            else
            {
                OnWateringAttempt?.Invoke(false);
                StartCoroutine(ShowResultAndEnd(1.5f));
            }
        }

        private IEnumerator ShowResultAndEnd(float delay)
        {
            yield return new WaitForSeconds(delay);
            EndMiniGame();
        }

        private string CheckWaterZone()
        {
            if (waterPosition >= greenZoneMin && waterPosition <= greenZoneMax)
            {
                UpdateInstructionText("🌸 Цветок полит!");
                return "success";
            }

            if (waterPosition >= yellowZoneMin && waterPosition <= yellowZoneMax)
            {
                UpdateInstructionText("🌼 Цветок полит!");
                return "warning";
            }

            if ((waterPosition >= darkRedZoneMin && waterPosition <= darkRedZoneMax) ||
                (waterPosition >= brightRedZoneMin && waterPosition <= brightRedZoneMax))
            {
                UpdateInstructionText("💀 Сейчас завянет!");
                return "fail";
            }

            return "fail";
        }

        private void UpdateInstructionText(string message)
        {
            if (instructionText != null)
            {
                instructionText.text = message;
            }
        }
    }
}