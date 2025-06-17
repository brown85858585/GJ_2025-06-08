using System.Collections;
using Game.MiniGames;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks.Triggers;

public class FlowerMiniGameManager : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private GameObject panel;

    public GameObject Panel => panel;

    private GameObject miniGamePanel;

    [Header("UI Elements")]
    private RectTransform indicator;
    private RectTransform trackBackground;
    private Button actionButton;
    private Button exitButton;
    private Button startButton; // Новая кнопка старта
    private Button startExitButton;
    private Text instructionText;
    private GameObject startScreen; // Стартовый экран
    private GameObject gameScreen;  // Игровой экран

    [Header("Game Settings")]
    public float indicatorSpeed = 100f; // Уменьшена скорость
    public float trackHeight = 300f;
    public float trackWidth = 60f;
    public float zoneHeight = 75f;
    public int maxAttempts = 3;

    [Header("Colors")]
    public Color darkRedColor = new Color(0.6f, 0f, 0f);
    public Color yellowZoneColor = Color.yellow;
    public Color greenZoneColor = Color.green;
    public Color brightRedColor = Color.red;
    public Color indicatorColor = Color.black;

    private bool isGameActive = false;
    private bool isMovingUp = true;
    private int currentAttempts = 0;
    private float indicatorPosition = 0f;
    private float trackTop, trackBottom;
    private bool gameStarted = false; // Флаг для отслеживания состояния

    // Зоны для проверки
    private RectTransform darkRedZone;
    private RectTransform yellowZone;
    private RectTransform greenZone;
    private RectTransform brightRedZone;

    [Header("Input")]
    [SerializeField] private InputActionReference actionInputAction; // E клавиша
    [SerializeField] private InputActionReference startInputAction;  // Пробел для старта

    // Если InputActionReference не назначены, используем коды клавиш
    private bool useDirectInput = false;

    // События
    public System.Action OnMiniGameComplete;
    public System.Action<bool> OnWateringAttempt;

    void Start()
    {
      
        FindSceneComponents();
        SetupInput();

        // Убедиться что панель выключена при старте
        if (miniGamePanel != null)
        {
            miniGamePanel.SetActive(false);
            Debug.Log("MiniGamePanel выключена при инициализации");
        }

        CreateMiniGameUI();
    }

    private void SetupInput()
    {
        // Проверить есть ли назначенные InputActionReference
        if (actionInputAction == null || startInputAction == null)
        {
            useDirectInput = true;
            Debug.Log("InputActionReference не назначены, используем прямой ввод клавиш");
            return;
        }

        // Подписаться на события Input System
        actionInputAction.action.performed += OnActionInput;
        startInputAction.action.performed += OnStartInput;

        Debug.Log("Input System настроена для мини-игры");
    }

    private void OnDestroy()
    {
        // Отписаться от событий при уничтожении объекта
        if (actionInputAction != null)
            actionInputAction.action.performed -= OnActionInput;

        if (startInputAction != null)
            startInputAction.action.performed -= OnStartInput;
    }

    private void OnActionInput(InputAction.CallbackContext context)
    {
        // Обработка клавиши действия (E) через Input System
        if (gameStarted && isGameActive)
        {
            Debug.Log("E нажата через Input System!");
            OnActionButtonClick();
        }
    }

    private void OnStartInput(InputAction.CallbackContext context)
    {
        // Обработка клавиши старта (Пробел) через Input System
        if (!gameStarted)
        {
            Debug.Log("Пробел нажат через Input System - запуск игры!");
            StartGame();
        }
    }

    void Update()
    {
        // Использовать прямой ввод только если Input System не настроена
        if (!useDirectInput) return;
        /*
        // Fallback на старую Input System если InputActionReference не назначены
        if (gameStarted && isGameActive && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("E нажата через старую Input System!");
            OnActionButtonClick();
        }

        if (!gameStarted && Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Пробел нажат через старую Input System - запуск игры!");
            StartGame();
        }
        */
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

        Debug.Log($"Компоненты найдены: Canvas = {mainCanvas.name}, Panel = {miniGamePanel.name}");
    }

    private void CreateMiniGameUI()
    {
        if (miniGamePanel == null) return;

        // УБЕДИТЬСЯ что панель выключена при инициализации
        miniGamePanel.SetActive(false);

        // Очистить существующие элементы (если есть)
        ClearExistingElements();

        // Создать стартовый экран
        CreateStartScreen();

        // Создать игровой экран (но скрыть его)
        CreateGameScreen();

        Debug.Log("Мини-игра с стартовым экраном готова!");
    }

    private void CreateStartScreen()
    {
        // Контейнер для стартового экрана
        startScreen = new GameObject("StartScreen");
        startScreen.transform.SetParent(miniGamePanel.transform, false);

        RectTransform startRect = startScreen.AddComponent<RectTransform>();
        startRect.anchorMin = Vector2.zero;
        startRect.anchorMax = Vector2.one;
        startRect.offsetMin = Vector2.zero;
        startRect.offsetMax = Vector2.zero;

        // Фон стартового экрана
        Image startBg = startScreen.AddComponent<Image>();
        startBg.color = new Color(0, 0, 0, 0.7f); // Полупрозрачный черный

        // Заголовок
        CreateStartText("Мини-игра: Полив цветка", new Vector2(0, 80), 24, Color.white);

        // Инструкции
        CreateStartText("Остановите индикатор в зеленой или желтой зоне", new Vector2(0, 40), 16, Color.yellow);
        CreateStartText("🟢 Зеленая зона = отлично", new Vector2(0, 10), 14, Color.green);
        CreateStartText("🟡 Желтая зона = хорошо", new Vector2(0, -10), 14, Color.yellow);
        CreateStartText("🔴 Красная зона = плохо", new Vector2(0, -30), 14, Color.red);

        // Кнопка старта
        startButton = CreateStartButton("StartButton", "Начать игру (Пробел)", new Vector2(0, -80), new Color(0.2f, 0.8f, 0.2f), new Vector2(200, 50));
        startButton.onClick.AddListener(StartGame);

        // Кнопка выхода на стартовом экране
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

    private void CreateGameScreen()
    {

        // Контейнер для игрового экрана
        gameScreen = new GameObject("GameScreen");
        gameScreen.transform.SetParent(miniGamePanel.transform, false);
        gameScreen.SetActive(false); // Изначально скрыт

        RectTransform gameRect = gameScreen.AddComponent<RectTransform>();
        gameRect.anchorMin = Vector2.zero;
        gameRect.anchorMax = Vector2.one;
        gameRect.offsetMin = Vector2.zero;
        gameRect.offsetMax = Vector2.zero;

        // Теперь все игровые элементы будут родителями gameScreen вместо miniGamePanel
        CreateVerticalTrack();
        CreateColorZones();
        CreateVerticalIndicator();
        CreateGameButtons();
        CreateInstructionText();
        CalculateTrackBounds();
    }

    private void CreateVerticalTrack()
    {
        GameObject trackObj = new GameObject("Track");
        trackObj.transform.SetParent(gameScreen.transform, false); // Родитель = gameScreen

        Image trackImage = trackObj.AddComponent<Image>();
        trackImage.color = Color.clear;

        trackBackground = trackObj.GetComponent<RectTransform>();
        trackBackground.sizeDelta = new Vector2(trackWidth, trackHeight);
        trackBackground.anchoredPosition = Vector2.zero;
    }

    private void CreateColorZones()
    {
        float startY = trackHeight / 2f - zoneHeight / 2f;

        darkRedZone = CreateZone("DarkRedZone", darkRedColor, new Vector2(0, startY), new Vector2(trackWidth, zoneHeight));
        yellowZone = CreateZone("YellowZone", yellowZoneColor, new Vector2(0, startY - zoneHeight), new Vector2(trackWidth, zoneHeight));
        greenZone = CreateZone("GreenZone", greenZoneColor, new Vector2(0, startY - zoneHeight * 2), new Vector2(trackWidth, zoneHeight));
        brightRedZone = CreateZone("BrightRedZone", brightRedColor, new Vector2(0, startY - zoneHeight * 3), new Vector2(trackWidth, zoneHeight));
    }

    private RectTransform CreateZone(string name, Color color, Vector2 position, Vector2 size)
    {
        GameObject zoneObj = new GameObject(name);
        zoneObj.transform.SetParent(gameScreen.transform, false); // Родитель = gameScreen

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
        indicatorObj.transform.SetParent(gameScreen.transform, false); // Родитель = gameScreen

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
        // Кнопка действия (нажми E)
        actionButton = CreateButton("ActionButton", "Нажми E", new Vector2(-100, -trackHeight / 2f - 50), new Color(0.2f, 0.6f, 1f), new Vector2(80, 40));
        actionButton.onClick.AddListener(OnActionButtonClick);
        actionButton.transform.SetParent(gameScreen.transform, false);

        // Кнопка выхода
        exitButton = CreateButton("ExitButton", "Выход", new Vector2(100, -trackHeight / 2f - 50), Color.gray, new Vector2(80, 40));
        exitButton.onClick.AddListener(ExitMiniGame);
        exitButton.transform.SetParent(gameScreen.transform, false);
    }

    private Button CreateStartButton(string name, string text, Vector2 position, Color color, Vector2 size)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(startScreen.transform, false); // ← ВАЖНО! Родитель = startScreen

        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = color;

        Button button = buttonObj.AddComponent<Button>();

        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.sizeDelta = size;
        buttonRect.anchoredPosition = position;

        // Текст на кнопке
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

    private Button CreateButton(string name, string text, Vector2 position, Color color, Vector2 size)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(gameScreen.transform, false); // Временно, потом переназначим

        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = color;

        Button button = buttonObj.AddComponent<Button>();

        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.sizeDelta = size;
        buttonRect.anchoredPosition = position;

        // Текст на кнопке
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
        textObj.transform.SetParent(gameScreen.transform, false); // Родитель = gameScreen

        instructionText = textObj.AddComponent<Text>();
        instructionText.text = "Нажми E в нужный момент!";
        instructionText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        instructionText.alignment = TextAnchor.MiddleCenter;
        instructionText.color = Color.black;
        instructionText.fontSize = 16;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(250, 40);
        textRect.anchoredPosition = new Vector2(trackWidth + 130, trackHeight / 2f);
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

    private void CalculateTrackBounds()
    {
        trackBottom = -trackHeight / 2f;
        trackTop = trackHeight / 2f;
    }

    // ПУБЛИЧНЫЕ МЕТОДЫ

    public void StartMiniGame()
    {
        if (miniGamePanel == null)
        {
            Debug.LogError("Мини-игра не инициализирована!");
            return;
        }

        Debug.Log("🎮 Открытие мини-игры (стартовый экран)");

        // Включить Input Actions если используется новая система
        if (!useDirectInput)
        {
            actionInputAction?.action.Enable();
            startInputAction?.action.Enable();
        }

        miniGamePanel.SetActive(true);
        if (startScreen != null)
        {
            startScreen.SetActive(true);
            Debug.Log("Стартовый экран скрыт!");
        }

        // Показать игровой экран
        if (gameScreen != null)
        {
            gameScreen.SetActive(false);
            Debug.Log("Игровой экран показан!");
        }

        // ВКЛЮЧИТЬ панель с стартовым экраном




        gameStarted = false;
        isGameActive = false;

        Debug.Log("Стартовый экран показан!");
    }

    private void StartGame()
    {
        Debug.Log("🎮 Запуск игры!");

        // Скрыть стартовый экран, показать игровой
        if (startScreen != null)
        {
            startScreen.SetActive(false);
            Debug.Log("Стартовый экран скрыт!");
        }

        // Показать игровой экран
        if (gameScreen != null)
        {
            gameScreen.SetActive(true);
            Debug.Log("Игровой экран показан!");
        }

        gameStarted = true;
        currentAttempts = 0;
        isGameActive = true;

        ResetIndicator();
        StartCoroutine(MoveIndicatorVertically());
    }

    public void EndMiniGame()
    {
        Debug.Log("🎮 Завершение мини-игры");

        isGameActive = false;
        gameStarted = false;

        // Отключить Input Actions если используется новая система
        if (!useDirectInput)
        {
            actionInputAction?.action.Disable();
            startInputAction?.action.Disable();
        }

        // ВЫКЛЮЧИТЬ панель при завершении
        if (miniGamePanel != null)
        {
            miniGamePanel.SetActive(false);
            Debug.Log("MiniGamePanel выключена!");
        }

        OnMiniGameComplete?.Invoke();
    }

    private void ExitMiniGame()
    {
        Debug.Log("Выход из мини-игры");
        EndMiniGame();
    }

    // ЛОГИКА ИГРЫ (остается без изменений)

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
                    OnActionButtonClick();
                }

                indicator.anchoredPosition = new Vector2(0, indicatorPosition);
            }

            yield return null;
        }
    }

    private void OnActionButtonClick()
    {
        if (!isGameActive)
        {
            Debug.Log("Игра неактивна, игнорируем нажатие E");
            return;
        }

        Debug.Log("✅ E обработана! Останавливаем индикатор...");

        isGameActive = false;
        isMovingUp = false;

        string result = CheckIndicatorZone();

        if (result == "success")
        {
            Debug.Log("✅ Цветок полит!");
            OnWateringAttempt?.Invoke(true);
            StartCoroutine(ShowResultAndEnd(1f));
        }
        else if (result == "warning")
        {
            Debug.Log("⚠️ Цветок полит, но не идеально!");
            OnWateringAttempt?.Invoke(true);
            StartCoroutine(ShowResultAndEnd(1f));
        }
        else
        {
            Debug.Log($"❌ Сейчас завянет!");
            OnWateringAttempt?.Invoke(false);
            StartCoroutine(ShowResultAndEnd(1.5f));
        }
    }

    private IEnumerator ShowResultAndEnd(float delay)
    {
        yield return new WaitForSeconds(delay);
        EndMiniGame();
    }

    private string CheckIndicatorZone()
    {
        if (indicator == null) return "fail";

        float indicatorY = indicator.anchoredPosition.y;

        if (IsInZone(indicatorY, greenZone))
        {
            UpdateInstructionText("🌸 Цветок полит!");
            return "success";
        }

        if (IsInZone(indicatorY, yellowZone))
        {
            UpdateInstructionText("🌼 Цветок полит!");
            return "warning";
        }

        if (IsInZone(indicatorY, darkRedZone) || IsInZone(indicatorY, brightRedZone))
        {
            UpdateInstructionText("💀 Сейчас завянет!");
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

    private void UpdateInstructionText(string message)
    {
        if (instructionText != null)
        {
            instructionText.text = message;
        }
    }
}