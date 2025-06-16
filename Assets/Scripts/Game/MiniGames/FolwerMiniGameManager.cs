using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FolwerMiniGameManager : MonoBehaviour
{
    [Header("Scene References")]
    private Canvas mainCanvas;
    private GameObject miniGamePanel;

    [Header("UI Elements")]
    private RectTransform indicator;
    private RectTransform trackBackground;
    private Button actionButton;
    private Button exitButton;
    private Text instructionText;

    [Header("Game Settings")]
    public float indicatorSpeed = 200f;
    public float trackHeight = 300f; // Уменьшили для помещения на экран
    public float trackWidth = 60f;
    public float zoneHeight = 75f; // Высота каждой зоны
    public int maxAttempts = 3;

    [Header("Colors")]
    public Color darkRedColor = new Color(0.6f, 0f, 0f); // Темно-красный
    public Color yellowZoneColor = Color.yellow;
    public Color greenZoneColor = Color.green;
    public Color brightRedColor = Color.red; // Ярко-красный
    public Color indicatorColor = Color.black;

    private bool isGameActive = false;
    private bool isMovingUp = true;
    private int currentAttempts = 0;
    private float indicatorPosition = 0f;
    private float trackTop, trackBottom;

    // Зоны для проверки
    private RectTransform darkRedZone;
    private RectTransform yellowZone;
    private RectTransform greenZone;
    private RectTransform brightRedZone;

    // События
    public System.Action OnMiniGameComplete;
    public System.Action<bool> OnWateringAttempt;

    void Start()
    {
        FindSceneComponents();

        // Убедиться что панель выключена при старте
        if (miniGamePanel != null)
        {
            miniGamePanel.SetActive(false);
            Debug.Log("MiniGamePanel выключена при инициализации");
        }

        CreateMiniGameUI();
    }

    void Update()
    {
        // Обработка клавиши E во время активной игры
        if (isGameActive && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("E нажата в MiniGameController!");
            OnActionButtonClick();
        }
    }

    private void FindSceneComponents()
    {
        mainCanvas = FindObjectOfType<Canvas>();
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

        // Создать вертикальный трек
        CreateVerticalTrack();

        // Создать зоны (красные, зеленая, желтая)
        CreateColorZones();

        // Создать индикатор
        CreateVerticalIndicator();

        // Создать кнопки и текст
        CreateButtons();
        CreateInstructionText();

        // Вычислить границы трека
        CalculateTrackBounds();

        Debug.Log("Вертикальная мини-игра готова!");
    }

    private void ClearExistingElements()
    {
        // Удалить старые элементы если они есть
        Transform[] children = miniGamePanel.GetComponentsInChildren<Transform>();
        for (int i = children.Length - 1; i >= 0; i--)
        {
            if (children[i] != miniGamePanel.transform)
            {
                DestroyImmediate(children[i].gameObject);
            }
        }
    }

    private void CreateVerticalTrack()
    {
        GameObject trackObj = new GameObject("Track");
        trackObj.transform.SetParent(miniGamePanel.transform, false);

        Image trackImage = trackObj.AddComponent<Image>();
        trackImage.color = Color.clear; // ← УБИРАЕМ БЕЛЫЙ ФОН! Делаем прозрачным

        trackBackground = trackObj.GetComponent<RectTransform>();
        trackBackground.sizeDelta = new Vector2(trackWidth, trackHeight);
        trackBackground.anchoredPosition = Vector2.zero;
    }

    private void CreateColorZones()
    {
        // 4 зоны сверху вниз: темно-красная, желтая, зеленая, ярко-красная
        float startY = trackHeight / 2f - zoneHeight / 2f;

        // Темно-красная зона (сверху)
        darkRedZone = CreateZone("DarkRedZone", darkRedColor, new Vector2(0, startY), new Vector2(trackWidth, zoneHeight));

        // Желтая зона
        yellowZone = CreateZone("YellowZone", yellowZoneColor, new Vector2(0, startY - zoneHeight), new Vector2(trackWidth, zoneHeight));

        // Зеленая зона
        greenZone = CreateZone("GreenZone", greenZoneColor, new Vector2(0, startY - zoneHeight * 2), new Vector2(trackWidth, zoneHeight));

        // Ярко-красная зона (снизу)
        brightRedZone = CreateZone("BrightRedZone", brightRedColor, new Vector2(0, startY - zoneHeight * 3), new Vector2(trackWidth, zoneHeight));
    }

    private RectTransform CreateZone(string name, Color color, Vector2 position, Vector2 size)
    {
        GameObject zoneObj = new GameObject(name);
        zoneObj.transform.SetParent(miniGamePanel.transform, false);

        Image zoneImage = zoneObj.AddComponent<Image>();
        zoneImage.color = new Color(color.r, color.g, color.b, 0.7f); // Полупрозрачность

        RectTransform zoneRect = zoneObj.GetComponent<RectTransform>();
        zoneRect.sizeDelta = size;
        zoneRect.anchoredPosition = position;

        return zoneRect;
    }

    private void CreateVerticalIndicator()
    {
        GameObject indicatorObj = new GameObject("Indicator");
        indicatorObj.transform.SetParent(miniGamePanel.transform, false);

        Image indicatorImage = indicatorObj.AddComponent<Image>();
        indicatorImage.color = indicatorColor;

        indicator = indicatorObj.GetComponent<RectTransform>();
        indicator.sizeDelta = new Vector2(trackWidth + 10, 15); // Немного шире трека
        indicator.anchoredPosition = new Vector2(0, -trackHeight / 2f);

        // Добавить стрелку (треугольник справа)
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

        // Можно добавить спрайт треугольника или использовать простой квадрат
    }

    private void CreateButtons()
    {
        // Кнопка действия (нажми E)
        actionButton = CreateButton("ActionButton", "Нажми E", new Vector2(-100, -trackHeight / 2f - 50), new Color(0.2f, 0.6f, 1f));
        actionButton.onClick.AddListener(OnActionButtonClick);

        // Кнопка выхода
        exitButton = CreateButton("ExitButton", "Выход", new Vector2(100, -trackHeight / 2f - 50), Color.gray);
        exitButton.onClick.AddListener(OnExitButtonClick);
    }

    private Button CreateButton(string name, string text, Vector2 position, Color color)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(miniGamePanel.transform, false);

        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = color;

        Button button = buttonObj.AddComponent<Button>();

        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(80, 40);
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
        textObj.transform.SetParent(miniGamePanel.transform, false);

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

        Debug.Log("🎮 Запуск вертикальной мини-игры");

        // ВКЛЮЧИТЬ панель при запуске мини-игры
        miniGamePanel.SetActive(true);
        Debug.Log("MiniGamePanel включена!");

        currentAttempts = 0;
        isGameActive = true;

        ResetIndicator();
        StartCoroutine(MoveIndicatorVertically());
    }

    public void EndMiniGame()
    {
        Debug.Log("🎮 Завершение мини-игры");

        isGameActive = false;

        // ВЫКЛЮЧИТЬ панель при завершении
        if (miniGamePanel != null)
        {
            miniGamePanel.SetActive(false);
            Debug.Log("MiniGamePanel выключена!");
        }

        OnMiniGameComplete?.Invoke();
    }

    // ЛОГИКА ИГРЫ

    private void ResetIndicator()
    {
        if (indicator != null)
        {
            indicatorPosition = trackBottom;
            indicator.anchoredPosition = new Vector2(0, indicatorPosition);
            isMovingUp = true;
            isGameActive = true; // Включить движение
        }
    }

    private IEnumerator MoveIndicatorVertically()
    {
        while (isGameActive && isMovingUp)
        {
            if (indicator != null)
            {
                // Движение вверх
                indicatorPosition += indicatorSpeed * Time.deltaTime;

                // Проверка достижения верха
                if (indicatorPosition >= trackTop)
                {
                    indicatorPosition = trackTop;
                    isMovingUp = false;

                    // Автоматически завершить игру если достиг верха
                    yield return new WaitForSeconds(0.5f);
                    Debug.Log("Индикатор достиг верха!");
                    OnActionButtonClick(); // Автоматический "промах"
                }

                // Обновить позицию
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

        // ОСТАНОВИТЬ движение индикатора
        isGameActive = false;
        isMovingUp = false;

        // Определить в какой зоне находится индикатор
        string result = CheckIndicatorZone();

        if (result == "success")
        {
            Debug.Log("✅ Цветок полит!");
            OnWateringAttempt?.Invoke(true);

            // Показать результат на 1 секунду, затем закрыть
            StartCoroutine(ShowResultAndEnd(1f));
        }
        else if (result == "warning")
        {
            Debug.Log("⚠️ Цветок полит, но не идеально!");
            OnWateringAttempt?.Invoke(true);

            // Показать результат на 1 секунду, затем закрыть
            StartCoroutine(ShowResultAndEnd(1f));
        }
        else
        {
            currentAttempts++;
            Debug.Log($"❌ Сейчас завянет! Попытка {currentAttempts}/{maxAttempts}");
            OnWateringAttempt?.Invoke(false);

            if (currentAttempts >= maxAttempts)
            {
                Debug.Log("Попытки закончились!");
                StartCoroutine(ShowResultAndEnd(1.5f));
            }
            else
            {
                // Показать результат на 1 секунду, затем перезапустить
                StartCoroutine(ShowResultAndRestart(1f));
            }
        }
    }

    private IEnumerator ShowResultAndEnd(float delay)
    {
        yield return new WaitForSeconds(delay);
        EndMiniGame();
    }

    private IEnumerator ShowResultAndRestart(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Сбросить текст инструкции
        UpdateInstructionText("Нажми E в нужный момент!");

        // Перезапустить движение
        ResetIndicator();
        StartCoroutine(MoveIndicatorVertically());
    }

    private string CheckIndicatorZone()
    {
        if (indicator == null) return "fail";

        float indicatorY = indicator.anchoredPosition.y;

        // Проверить зеленую зону (лучший результат)
        if (IsInZone(indicatorY, greenZone))
        {
            UpdateInstructionText("🌸 Цветок полит!");
            return "success";
        }

        // Проверить желтую зону (хороший результат)
        if (IsInZone(indicatorY, yellowZone))
        {
            UpdateInstructionText("🌼 Цветок полит!");
            return "warning";
        }

        // Проверить красные зоны (плохой результат)
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

    private void OnExitButtonClick()
    {
        Debug.Log("Выход из мини-игры");
        EndMiniGame();
    }

    // Удалить старый метод CheckIfInGreenZone - заменен на CheckIndicatorZone
}