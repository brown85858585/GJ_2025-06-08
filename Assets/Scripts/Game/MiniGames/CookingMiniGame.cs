using Game.MiniGames;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CookingMiniGame : BaseTimingMiniGame
{
    [Header("Cooking Settings")]
    public float arcRadius = 150f;
    public float arcStartAngle = 200f; // Начальный угол дуги
    public float arcEndAngle = 340f;   // Конечный угол дуги
    public float successZoneAngle = 45f; // Размер зеленой зоны в градусах

    [Header("Colors")]
    public Color arcColor = Color.gray;
    public Color successZoneColor = Color.green;
    public Color indicatorColor = Color.black;

    private RectTransform arcBackground;
    private RectTransform successZone;
    private float currentAngle;
    private float targetAngle; // Угол для зеленой зоны
    private bool movingClockwise = true;

    protected override void CreateStartScreen()
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
        startBg.color = new Color(0, 0, 0, 0.7f);

        // Заголовок и инструкции
        CreateText("Title", "🍳 Готовка еды", new Vector2(0, 100), 24, Color.white, new Vector2(400, 40), startScreen.transform);
        CreateText("Subtitle", "Алгоритм:", new Vector2(0, 60), 18, Color.yellow, new Vector2(400, 30), startScreen.transform);

        CreateText("Step1", "1. Игрок нажимает на кухню", new Vector2(0, 20), 14, Color.white, new Vector2(400, 25), startScreen.transform);
        CreateText("Step2", "2. ГГ подходит к холодильнику, берет пакет, переносит его на стол", new Vector2(0, -5), 14, Color.white, new Vector2(500, 25), startScreen.transform);
        CreateText("Step3", "3. Запускается анимация нарезки продуктов", new Vector2(0, -30), 14, Color.white, new Vector2(400, 25), startScreen.transform);
        CreateText("Step4", "4. Мини-игра: остановите стрелку в зеленой зоне", new Vector2(0, -55), 14, Color.green, new Vector2(400, 25), startScreen.transform);
        CreateText("Step5", "5. После завершения ГГ садится за стол и ест", new Vector2(0, -80), 14, Color.white, new Vector2(400, 25), startScreen.transform);

        // Кнопки
        startButton = CreateButton("StartButton", "Начать готовку (Пробел)", new Vector2(0, -130), new Color(0.2f, 0.8f, 0.2f), new Vector2(220, 50), startScreen.transform);
        startButton.onClick.AddListener(StartGame);

        Button startExitButton = CreateButton("StartExitButton", "Выход", new Vector2(0, -190), Color.gray, new Vector2(120, 40), startScreen.transform);
        startExitButton.onClick.AddListener(ExitMiniGame);
    }

    protected override void CreateGameScreen()
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

        // Создать дугу и индикатор
        CreateArc();
        CreateSuccessZone();
        CreateIndicator();
        CreateGameButtons();
        CreateInstructionText();
    }

    private void CreateArc()
    {
        // Базовая дуга (серая) - используем Image из базового класса
        GameObject arcObj = CreateImageObject("Arc", arcImage, new Vector2(arcRadius * 2, arcRadius * 2), Vector2.zero);

        Image currentArcImage = arcObj.GetComponentInChildren<Image>();
        if (currentArcImage == null)
        {
            currentArcImage = arcObj.GetComponent<Image>();
        }

        currentArcImage.color = arcColor;

        // Настройка для отображения дуги
        currentArcImage.sprite = CreateCircleSprite();
        currentArcImage.type = Image.Type.Filled;
        currentArcImage.fillMethod = Image.FillMethod.Radial360;
        currentArcImage.fillOrigin = 2; // Top
        currentArcImage.fillAmount = (arcEndAngle - arcStartAngle) / 360f;

        arcBackground = arcObj.GetComponent<RectTransform>();

        // Поворот для правильного позиционирования дуги
        arcBackground.rotation = Quaternion.Euler(0, 0, -arcStartAngle);
    }

    private void CreateSuccessZone()
    {
        // Зеленая зона успеха - используем Image из базового класса
        GameObject successObj = CreateImageObject("SuccessZone", successZoneImage, new Vector2(arcRadius * 2, arcRadius * 2), Vector2.zero);

        Image currentSuccessImage = successObj.GetComponentInChildren<Image>();
        if (currentSuccessImage == null)
        {
            currentSuccessImage = successObj.GetComponent<Image>();
        }

        currentSuccessImage.color = successZoneColor;
        currentSuccessImage.sprite = CreateCircleSprite();
        currentSuccessImage.type = Image.Type.Filled;
        currentSuccessImage.fillMethod = Image.FillMethod.Radial360;
        currentSuccessImage.fillOrigin = 2;
        currentSuccessImage.fillAmount = successZoneAngle / 360f;

        successZone = successObj.GetComponent<RectTransform>();

        // Случайная позиция для зеленой зоны
        targetAngle = Random.Range(arcStartAngle + successZoneAngle / 2, arcEndAngle - successZoneAngle / 2);
        successZone.rotation = Quaternion.Euler(0, 0, -targetAngle);
    }

    private void CreateIndicator()
    {
        // Стрелка-индикатор - используем Image из базового класса
        GameObject indicatorObj = CreateImageObject("Indicator", indicatorImage, new Vector2(10, arcRadius + 20), Vector2.zero);

        Image currentIndicatorImage = indicatorObj.GetComponentInChildren<Image>();
        if (currentIndicatorImage == null)
        {
            currentIndicatorImage = indicatorObj.GetComponent<Image>();
        }

        currentIndicatorImage.color = indicatorColor;

        indicator = indicatorObj.GetComponent<RectTransform>();
        indicator.pivot = new Vector2(0.5f, 0f); // Поворот от основания

        // Начальная позиция
        currentAngle = arcStartAngle;
        UpdateIndicatorPosition();
    }

    private void CreateGameButtons()
    {
        // Кнопка действия
        actionButton = CreateButton("ActionButton", "Остановить (E)", new Vector2(-100, -200), new Color(0.2f, 0.6f, 1f), new Vector2(120, 40), gameScreen.transform);
        actionButton.onClick.AddListener(OnActionButtonClick);

        // Кнопка выхода
        exitButton = CreateButton("ExitButton", "Выход", new Vector2(100, -200), Color.gray, new Vector2(80, 40), gameScreen.transform);
        exitButton.onClick.AddListener(ExitMiniGame);
    }

    private void CreateInstructionText()
    {
        instructionText = CreateText("InstructionText", "Остановите стрелку в зеленой зоне!", new Vector2(0, 200), 16, Color.black, new Vector2(300, 40), gameScreen.transform);
    }

    private Sprite CreateCircleSprite()
    {
        // Создаем простой белый круг
        Texture2D texture = new Texture2D(100, 100);
        Color[] pixels = new Color[100 * 100];

        Vector2 center = new Vector2(50, 50);
        float radius = 45f;

        for (int y = 0; y < 100; y++)
        {
            for (int x = 0; x < 100; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance <= radius && distance >= radius - 10) // Кольцо
                {
                    pixels[y * 100 + x] = Color.white;
                }
                else
                {
                    pixels[y * 100 + x] = Color.clear;
                }
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, 100, 100), new Vector2(0.5f, 0.5f));
    }

    protected override void StartGameLogic()
    {
        // Сбросить позицию и начать движение
        currentAngle = arcStartAngle;
        movingClockwise = true;
        UpdateIndicatorPosition();

        StartCoroutine(MoveIndicator());
    }

    private IEnumerator MoveIndicator()
    {
        while (isGameActive)
        {
            // Движение стрелки
            float angleSpeed = indicatorSpeed * Time.deltaTime;

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

            UpdateIndicatorPosition();
            yield return null;
        }
    }

    private void UpdateIndicatorPosition()
    {
        if (indicator != null)
        {
            indicator.rotation = Quaternion.Euler(0, 0, -currentAngle);
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

        string result = CheckResult();

        if (result == "success")
        {
            Debug.Log("✅ Еда приготовлена идеально!");
            UpdateInstructionText("🍽️ Идеально приготовлено!");
            OnGameAttempt?.Invoke(true);
            StartCoroutine(ShowResultAndEnd(1.5f));
        }
        else
        {
            Debug.Log("❌ Еда подгорела!");
            UpdateInstructionText("🔥 Еда подгорела!");
            OnGameAttempt?.Invoke(false);
            StartCoroutine(ShowResultAndEnd(1.5f));
        }
    }

    protected override string CheckResult()
    {
        // Проверить попадание в зеленую зону
        float zoneStart = targetAngle - successZoneAngle / 2f;
        float zoneEnd = targetAngle + successZoneAngle / 2f;

        if (currentAngle >= zoneStart && currentAngle <= zoneEnd)
        {
            return "success";
        }

        return "fail";
    }

    // Публичные методы для интеграции с системой готовки
    public void SetDifficulty(float speed, float zoneSize)
    {
        indicatorSpeed = speed;
        successZoneAngle = zoneSize;
    }

    public void StartCooking()
    {
        StartMiniGame();
    }
}