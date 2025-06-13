using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MiniGameController : MonoBehaviour
{
    [Header("Scene References")]
    private Canvas mainCanvas;
    private GameObject miniGamePanel;

    [Header("UI Elements")]
    private RectTransform indicator;
    private RectTransform greenZone;
    private RectTransform trackBackground;
    private Button actionButton;
    private Button exitButton;
    private Text instructionText;

    [Header("Game Settings")]
    public float indicatorSpeed = 300f;
    public float trackHeight = 400f;
    public float trackWidth = 60f;
    public float greenZoneHeight = 80f;
    public int maxAttempts = 3;

    [Header("Colors")]
    public Color redZoneColor = Color.red;
    public Color greenZoneColor = Color.green;
    public Color yellowZoneColor = Color.yellow;
    public Color indicatorColor = Color.black;

    private bool isGameActive = false;
    private bool isMovingUp = true;
    private int currentAttempts = 0;
    private float indicatorPosition = 0f;
    private float trackTop, trackBottom;

    // События
    public System.Action OnMiniGameComplete;
    public System.Action<bool> OnWateringAttempt;

    void Start()
    {
        FindSceneComponents();
        if (miniGamePanel != null)
        {
            miniGamePanel.SetActive(false);
            Debug.Log("MiniGamePanel выключена при инициализации");
        }
        CreateMiniGameUI();
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
        trackImage.color = Color.white;

        trackBackground = trackObj.GetComponent<RectTransform>();
        trackBackground.sizeDelta = new Vector2(trackWidth, trackHeight);
        trackBackground.anchoredPosition = Vector2.zero;
    }

    private void CreateColorZones()
    {
        float zoneHeight = trackHeight / 4f; // 4 зоны: красная, желтая, зеленая, красная

        // Верхняя красная зона
        CreateZone("RedZoneTop", redZoneColor, new Vector2(0, trackHeight / 4f), new Vector2(trackWidth, zoneHeight));

        // Желтая зона
        CreateZone("YellowZone", yellowZoneColor, new Vector2(0, 0), new Vector2(trackWidth, zoneHeight));

        // Зеленая зона (целевая)
        greenZone = CreateZone("GreenZone", greenZoneColor, new Vector2(0, -trackHeight / 4f), new Vector2(trackWidth, greenZoneHeight));

        // Нижняя красная зона
        CreateZone("RedZoneBottom", redZoneColor, new Vector2(0, -trackHeight / 2f + zoneHeight / 2f), new Vector2(trackWidth, zoneHeight));
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
        // Кнопка действия (полить)
        actionButton = CreateButton("ActionButton", "Полить", new Vector2(-100, -trackHeight / 2f - 50), Color.blue);
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
        instructionText.text = "Индикатор движется снизу вверх до конца. Вниз не опускается.";
        instructionText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        instructionText.alignment = TextAnchor.MiddleCenter;
        instructionText.color = Color.black;
        instructionText.fontSize = 14;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(300, 60);
        textRect.anchoredPosition = new Vector2(trackWidth + 150, 0);
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

        Debug.Log("Запуск вертикальной мини-игры");

        miniGamePanel.SetActive(true);
        currentAttempts = 0;
        isGameActive = true;

        ResetIndicator();
        StartCoroutine(MoveIndicatorVertically());
    }

    public void EndMiniGame()
    {
        Debug.Log("Завершение мини-игры");

        isGameActive = false;

        if (miniGamePanel != null)
        {
            miniGamePanel.SetActive(false);
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
        if (!isGameActive) return;

        Debug.Log("Попытка полива!");

        bool isInGreenZone = CheckIfInGreenZone();

        if (isInGreenZone)
        {
            Debug.Log("Успех! Попал в зеленую зону!");
            OnWateringAttempt?.Invoke(true);
            EndMiniGame();
        }
        else
        {
            currentAttempts++;
            Debug.Log($"Промах! Попытка {currentAttempts}/{maxAttempts}");
            OnWateringAttempt?.Invoke(false);

            if (currentAttempts >= maxAttempts)
            {
                Debug.Log("Попытки закончились!");
                EndMiniGame();
            }
            else
            {
                ResetIndicator();
                StartCoroutine(MoveIndicatorVertically());
            }
        }
    }

    private void OnExitButtonClick()
    {
        Debug.Log("Выход из мини-игры");
        EndMiniGame();
    }

    private bool CheckIfInGreenZone()
    {
        if (indicator == null || greenZone == null) return false;

        float indicatorY = indicator.anchoredPosition.y;
        float greenZoneBottom = greenZone.anchoredPosition.y - greenZoneHeight / 2f;
        float greenZoneTop = greenZone.anchoredPosition.y + greenZoneHeight / 2f;

        bool inZone = indicatorY >= greenZoneBottom && indicatorY <= greenZoneTop;
        Debug.Log($"Индикатор Y: {indicatorY}, Зеленая зона: {greenZoneBottom} - {greenZoneTop}, В зоне: {inZone}");

        return inZone;
    }
}
