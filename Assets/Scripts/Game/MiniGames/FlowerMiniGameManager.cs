using System.Collections;
using Game.MiniGames;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks.Triggers;
using System.Collections.Generic;
using static Cinemachine.DocumentationSortingAttribute;
using Knot.Localization.Components;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.SearchService;
using Player;

namespace Game.MiniGames
{

    public class FlowerMiniGameManager : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private Canvas mainCanvas;
        [SerializeField] private GameObject panel;

        [SerializeField] private GameObject CanRoot;

        public GameObject Panel => panel;

        private GameObject miniGamePanel;

        [Header("Prefab References")]
        [SerializeField] private GameObject flowerGameViewPrefab; // Префаб FlowerGameView
        private GameObject instantiatedFlowerView;

        [Header("Prefab Settings")]
        [SerializeField] private float prefabScale = 0.5f; // Масштаб префаба
        [SerializeField] private Vector2 prefabPosition = Vector2.zero; // Позиция префаба
        [SerializeField] private bool disablePrefabCanvas = true; // Отключать Canvas в префабе

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

       // [Header("Game Settings")]
      //  public float indicatorSpeed = 100f;
      //  public float trackHeight = 300f; // Высота зоны для воды
       // public float zoneHeight = 75f;
       // public int maxAttempts = 3;

       // [Header("Water Animation")]
       // [SerializeField] private float waterMinY = -750f; // Минимальная позиция воды
      //  [SerializeField] private float waterMaxY = -150f;  // Максимальная позиция воды
      //  [SerializeField] private float waterHorizontalRange = 50f; // Диапазон горизонтального движения
      //  [SerializeField] private float waterHorizontalSpeed = 2f; // Скорость горизонтального движения

        // Добавить переменную для горизонтального движения
        private float waterHorizontalPosition = 0f;
        private bool isMovingRight = true;

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
        private int level = 0;

        [Header("Fonts")]
        [SerializeField] private string ButtonFont = "LegacyRuntime.ttf";
        [SerializeField] private string TextFont = "LegacyRuntime.ttf";


        [Header("Input")]
        [SerializeField] private InputActionReference actionInputAction;
        [SerializeField] private InputActionReference startInputAction;
        private bool useDirectInput = false;

        // События
        public System.Action OnMiniGameComplete;
        public System.Action<bool> OnWateringAttempt;


        [SerializeField] FloweGameOptions floweGameOption;
        [SerializeField] List<FloweGameOptions> floweGameOptions;
        PlayerModel model;


        public void SetPlayer(PlayerModel model)
        {
            this.model = model;
        }

        void Start()
        {
            level = MiniGameCoordinator.DayLevel;
            FindSceneComponents();
            //SetupInput();

            if (miniGamePanel != null)
            {
                miniGamePanel.SetActive(false);
            }
            
            CreateMiniGameUI();
            SetupWaterZones();
        }


        private void SetupWaterCoordinates()
        {
            if (waterMask == null || waterImage == null) return;

            // Получить реальные размеры маски воды
            RectTransform waterRect = waterImage.GetComponent<RectTransform>();
            RectTransform maskRect = waterMask;

            // Рассчитать диапазон движения воды на основе размера маски
            float maskHeight = maskRect.rect.height;

            // Установить диапазон относительно маски
            //waterMinY = -maskHeight / 2f; // Низ маски
            //waterMaxY = maskHeight / 2f;  // Верх маски

            Debug.Log($"🔧 Настройка координат воды: maskHeight={maskHeight}, minY={floweGameOptions[level].waterMinY}, maxY={floweGameOptions[level].waterMaxY}");

            // Пересчитать зоны полива
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

               // var parent = WhaterCan.transform.parent;
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
            if (floweGameOptions[level].UsedPrefab != null)
            {
                instantiatedFlowerView = Instantiate(floweGameOptions[level].UsedPrefab, gameScreen.transform);

                // Настроить RectTransform для префаба
                RectTransform flowerRect = instantiatedFlowerView.GetComponent<RectTransform>();
                if (flowerRect != null)
                {
                    // ЦЕНТРИРОВАНИЕ - привязка к центру экрана
                    flowerRect.anchorMin = new Vector2(0.5f, 0.5f);
                    flowerRect.anchorMax = new Vector2(0.5f, 0.5f);
                    flowerRect.anchoredPosition = Vector2.zero; // Позиция в центре
                    flowerRect.localScale = new Vector3(0.8f, 0.8f, 1f); // Немного уменьшим
                }

                // Найти компоненты в префабе
                FindPrefabComponents();

                // НАСТРОЙКА КООРДИНАТ ПОЛИВА после поиска компонентов
                SetupWaterCoordinates();
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
            if (instantiatedFlowerView == null)
            {
                Debug.LogError("❌ instantiatedFlowerView == null!");
                return;
            }

            Debug.Log($"🔍 Поиск компонентов в префабе: {instantiatedFlowerView.name}");

            // Показать всю структуру префаба
            LogChildren(instantiatedFlowerView.transform, 0);

            // Найти маску и изображение воды
            Transform maskTransform = instantiatedFlowerView.transform.Find("Panel/Mask");
            Debug.Log($"🔍 Поиск Panel/Mask: {maskTransform}");

            if (maskTransform != null)
            {
                waterMask = maskTransform.GetComponent<RectTransform>();
                Debug.Log($"✅ Маска найдена: {waterMask}");

                Transform waterTransform = maskTransform.Find("Water");
                Debug.Log($"🔍 Поиск Water: {waterTransform}");

                if (waterTransform != null)
                {
                    waterImage = waterTransform.GetComponent<Image>();
                    Debug.Log($"✅ Вода найдена: {waterImage}");
                }
                else
                {
                    Debug.LogError("❌ Water не найден в Mask!");
                }
            }
            else
            {
                Debug.LogError("❌ Panel/Mask не найден!");
            }

            // Найти контейнер цветка (если нужно)
            Transform flowerTransform = instantiatedFlowerView.transform.Find("Panel");
            if (flowerTransform != null)
            {
                flowerContainer = flowerTransform.GetComponent<RectTransform>();
                Debug.Log($"✅ Panel найден: {flowerContainer}");
            }

            if (waterMask == null || waterImage == null)
            {
                Debug.LogError("❌ Не удалось найти компоненты воды в префабе!");
            }
            else
            {
                Debug.Log("✅ Все компоненты найдены успешно!");
            }
        }

        private void LogChildren(Transform parent, int depth)
        {
            string indent = new string(' ', depth * 2);
            Debug.Log($"{indent}📁 {parent.name}");

            for (int i = 0; i < parent.childCount; i++)
            {
                LogChildren(parent.GetChild(i), depth + 1);
            }
        }

        private void SetupWaterZones()
        {
            // Определить зоны в координатах воды (относительно размера маски)
            float totalRange = floweGameOptions[level].waterMaxY - floweGameOptions[level].waterMinY;
            float zoneSize = totalRange / 4f;

            // Снизу вверх: плохо -> хорошо -> отлично -> хорошо
            brightRedZoneMin = floweGameOptions[level].waterMinY;                    // Дно - плохо
            brightRedZoneMax = floweGameOptions[level].waterMinY + zoneSize;

            yellowZoneMin = floweGameOptions[level].waterMinY + zoneSize;           // Желтая - хорошо  
            yellowZoneMax = floweGameOptions[level].waterMinY + zoneSize * 2;

            greenZoneMin = floweGameOptions[level].waterMinY + zoneSize * 2;        // Зеленая - отлично
            greenZoneMax = floweGameOptions[level].waterMinY + zoneSize * 3;

            darkRedZoneMin = floweGameOptions[level].waterMinY + zoneSize * 3;      // Верх - плохо
            darkRedZoneMax = floweGameOptions[level].waterMaxY;

            Debug.Log($"🎯 Зоны: Красная1({brightRedZoneMin:F1}-{brightRedZoneMax:F1}) Желтая({yellowZoneMin:F1}-{yellowZoneMax:F1}) Зеленая({greenZoneMin:F1}-{greenZoneMax:F1}) Красная2({darkRedZoneMin:F1}-{darkRedZoneMax:F1})");
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

            //CreateStartText("Мини-игра: Полив цветка", new Vector2(0, 80), 24, Color.white);
            //CreateStartText("Остановите воду в нужной зоне", new Vector2(0, 40), 16, Color.yellow);
            //CreateStartText("🟢 Зеленая зона = отлично", new Vector2(0, 10), 14, Color.green);
            //CreateStartText("🟡 Желтая зона = хорошо", new Vector2(0, -10), 14, Color.yellow);
            //CreateStartText("🔴 Красная зона = плохо", new Vector2(0, -30), 14, Color.red);

            startButton = CreateStartButton("StartButton", "Начать игру (Пробел)", new Vector2(-200, 0), new Color(0.2f, 0.8f, 0.2f), new Vector2(300, 100), 24);
            startButton.onClick.AddListener(StartGame);

            startExitButton = CreateStartButton("StartExitButton", "Выход", new Vector2(200, 0), Color.gray, new Vector2(300, 100), 24);
            startExitButton.onClick.AddListener(ExitMiniGame);
        }

        private void CreateStartText(string text, Vector2 position, int fontSize, Color color)
        {
            GameObject textObj = new GameObject("StartText");
            textObj.transform.SetParent(startScreen.transform, false);

            Text startText = textObj.AddComponent<Text>();
            startText.text = text;
            startText.font = Resources.GetBuiltinResource<Font> (TextFont);
            startText.alignment = TextAnchor.MiddleCenter;
            startText.color = color;
            startText.fontSize = fontSize;

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(400, 30);
            textRect.anchoredPosition = position;

            //var component = textObj.AddComponent<KnotLocalizedUIText>();

        }

        private Button CreateStartButton(string name, string text, Vector2 position, Color color, Vector2 size, int fontSize = 12)
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
            buttonText.font = Resources.GetBuiltinResource<Font>(ButtonFont);
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.color = Color.white;
            buttonText.fontSize = fontSize;

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            //var component = textObj.AddComponent<KnotLocalizedUIText>();

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
            buttonText.font = Resources.GetBuiltinResource<Font>(ButtonFont);
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.color = Color.white;
            buttonText.fontSize = 12;

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            //var component = textObj.AddComponent<KnotLocalizedUIText>();

            return button;
        }

        private void CreateInstructionText()
        {
            GameObject textObj = new GameObject("InstructionText");
            textObj.transform.SetParent(gameScreen.transform, false);

            instructionText = textObj.AddComponent<Text>();
            instructionText.text = "Нажми E в нужный момент!";
            instructionText.font = Resources.GetBuiltinResource<Font>(TextFont);
            instructionText.alignment = TextAnchor.MiddleCenter;
            instructionText.color = Color.black;
            instructionText.fontSize = 16;

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(250, 40);
            textRect.anchoredPosition = new Vector2(0, 250);
            textObj.SetActive(false);
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

            SetupInput();
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


        private void SetObjectToForeground(GameObject obj)
        {
            // Пробуем Canvas
            Canvas canvas = obj.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.sortingOrder = 100;
                return;
            }

            // Пробуем SpriteRenderer
            SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = 100;
                return;
            }

            // Через Z-координату
            obj.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y, -10f);
        }

        private void SetObjectToBackground(GameObject obj)
        {
            // Аналогично, но с меньшими значениями
            Canvas canvas = obj.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.sortingOrder = 1;
                return;
            }

            SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = 1;
                return;
            }

            obj.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y, 0f);
        }



        private void StartGame()
        {
     
            var tryfind = FindObjectsOfType<Transform>();

            foreach (Transform t in tryfind)
            {
                if (t.name.Contains("RoomCamera"))
                {
                    Transform canRoot = t.Find("CanRoot");
                    if (canRoot != null)
                    {
                        CanRoot = canRoot.gameObject;
                        canRoot.gameObject.SetActive(true);

                       // SetObjectToBackground(gameScreen);
                       //  SetObjectToForeground(CanRoot);
                        break;
                    }
                }
            }
            //CanRt.GameObject().SetActive(true);

            Debug.Log("🎮 StartGame вызван!");

            if (startScreen != null)
            {
                startScreen.SetActive(false);
                Debug.Log("✅ Стартовый экран скрыт");
            }

            if (gameScreen != null)
            {
                gameScreen.SetActive(true);
                Debug.Log("✅ Игровой экран показан");

                // Проверить что префаб активен
                if (instantiatedFlowerView != null)
                {
                    Debug.Log($"✅ Префаб активен: {instantiatedFlowerView.activeSelf}");
                    instantiatedFlowerView.SetActive(true); // Принудительно активировать
                }
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
            CanRoot?.SetActive(false);
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
                waterPosition = floweGameOptions[level].waterMinY;
                waterHorizontalPosition = 0f; // Сброс горизонтальной позиции
                isMovingRight = true; // Сброс направления
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
                // Обновляем и вертикальную, и горизонтальную позицию
                waterRect.anchoredPosition = new Vector2(waterHorizontalPosition, waterPosition);
            }
        }

        private IEnumerator MoveWater()
        {
            while (isGameActive && isMovingUp)
            {
                if (waterImage != null)
                {
                    // Вертикальное движение (как было)
                    waterPosition += floweGameOptions[level].indicatorSpeed * Time.deltaTime;

                    // Горизонтальное движение влево-вправо
                    if (isMovingRight)
                    {
                        waterHorizontalPosition += floweGameOptions[level].waterHorizontalSpeed * floweGameOptions[level].indicatorSpeed * Time.deltaTime;
                        if (waterHorizontalPosition >= floweGameOptions[level].waterHorizontalRange)
                        {
                            waterHorizontalPosition = floweGameOptions[level].waterHorizontalRange;
                            isMovingRight = false;
                        }
                    }
                    else
                    {
                        waterHorizontalPosition -= floweGameOptions[level].waterHorizontalSpeed * floweGameOptions[level].indicatorSpeed * Time.deltaTime;
                        if (waterHorizontalPosition <= -floweGameOptions[level].waterHorizontalRange)
                        {
                            waterHorizontalPosition = -floweGameOptions[level].waterHorizontalRange;
                            isMovingRight = true;
                        }
                    }

                    // Проверка достижения верха
                    if (waterPosition >= floweGameOptions[level].waterMaxY)
                    {
                        waterPosition = floweGameOptions[level].waterMaxY;
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
                model.Score += 500;
                UpdateInstructionText("🌸 Цветок полит!");
                return "success";
            }

            if (waterPosition >= yellowZoneMin && waterPosition <= yellowZoneMax)
            {
                model.Score += 300;
                UpdateInstructionText("🌼 Цветок полит!");
                return "warning";
            }

            if ((waterPosition >= darkRedZoneMin && waterPosition <= darkRedZoneMax) ||
                (waterPosition >= brightRedZoneMin && waterPosition <= brightRedZoneMax))
            {
                model.Score += 50;
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