using Game.MiniGames;
using Knot.Localization.Components;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class CardSwipeMiniGame : BaseTimingMiniGame
{
    [Header("Card Game Settings")]
    public float swipeAnimationSpeed = 500f;
    public Vector2 cardSize = new Vector2(350, 450);
    public int targetScore = 5; // Нужно набрать 5 баллов для победы
    public int maxCards = 10; // Максимум карточек в игре

    [Header("Card Visual Settings")]
    public Color cardBackgroundColor = Color.white;
    public Color cardTextColor = Color.black;
    public Color senderCircleColor = Color.gray;
    public Color acceptButtonColor = Color.green;
    public Color rejectButtonColor = Color.red;

    [Header("Game Data")]
    [SerializeField] private List<CardData> gameCards = new List<CardData>();

    [Header("UI Prefabs")]
    [SerializeField] private GameObject currentCardPrefab; // Перетащите префаб в инспекторе
    /*
    [SerializeField] private Transform headerContainer;
    [SerializeField] private Transform contentContainer;
    [SerializeField] private Transform headerText;
    [SerializeField] private Transform contentText;
    */
    // UI компоненты
    private GameObject currentCard;
    private TextMeshProUGUI cardSenderText;
    private TextMeshProUGUI cardContentText;
    private Text cardCounterText;
    private Text scoreText;
    private Button acceptButton;
    private Button rejectButton;
    private GameObject resultScreen;

    // Состояние игры
    private int currentCardIndex = 0;
    private int correctAnswers = 0;
    private int incorrectAnswers = 0;
    private int cardsRemaining;
    private bool isProcessingCard = false;

    [System.Serializable]
    public class CardData
    {
        [Header("Card Header")]
        public string sender; // "От кого"

        [Header("Card Content")]
        public string content; // Основной текст

        [Header("Card Type")]
        public bool isWorkRelated; // true = принять (работа), false = удалить (личное)

        public CardData(string sender, string content, bool workRelated)
        {
            this.sender = sender;
            this.content = content;
            isWorkRelated = workRelated;
        }
    }

    protected override void Start()
    {
        if (gameCards.Count == 0)
        {
            InitializeDefaultCards();
        }

        cardsRemaining = gameCards.Count;
        base.Start();
    }

    private void InitializeDefaultCards()
    {
        gameCards.AddRange(new List<CardData>
        {
            // Рабочие задачи (принять - зеленая кнопка)
            new CardData("Начальник", "Подготовьте отчет по проекту к пятнице", true),
            new CardData("Клиент", "Нужно обсудить детали договора", true),
            new CardData("HR", "Заполните анкету для аттестации", true),
            new CardData("Коллега", "Помогите с код-ревью, пожалуйста", true),
            new CardData("Техподдержка", "Обновите антивирус на рабочем ПК", true),
            new CardData("Бухгалтерия", "Предоставьте документы для отчетности", true),
            
            // Личные дела (удалить - красная кнопка)
            new CardData("Мама", "Не забудь купить молоко по дороге домой", false),
            new CardData("Друг", "Давай встретимся вечером в кафе", false),
            new CardData("Интернет-магазин", "Скидка 50% на всё! Только сегодня!", false),
            new CardData("Соцсеть", "У вас 5 новых уведомлений", false),
            new CardData("Игра", "Ваша энергия восстановлена! Заходите играть", false),
            new CardData("YouTube", "Новое видео от вашего любимого блогера", false)
        });

        ShuffleCards();

        // Берем только нужное количество карточек
        if (gameCards.Count > maxCards)
        {
            gameCards = gameCards.GetRange(0, maxCards);
        }
        cardsRemaining = gameCards.Count;
    }

    private void ShuffleCards()
    {
        for (int i = 0; i < gameCards.Count; i++)
        {
            CardData temp = gameCards[i];
            int randomIndex = Random.Range(i, gameCards.Count);
            gameCards[i] = gameCards[randomIndex];
            gameCards[randomIndex] = temp;
        }
    }

    protected override void FindSceneComponents()
    {
        if (mainCanvas == null)
        {
            Debug.LogError("Canvas не найден в сцене!");
            return;
        }

        miniGamePanel = GameObject.Find("WorkMiniGamePanel");
        if (miniGamePanel == null)
        {
            Debug.LogError("MiniGamePanel не найдена в Canvas!");
            return;
        }

        Debug.Log($"Компоненты найдены: Canvas = {mainCanvas.name}, Panel = {miniGamePanel.name}");
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
        startBg.color = new Color(0, 0, 0, 0.8f);

        CreateText("Title", "📋 Работа", new Vector2(0, 180), 32, Color.white, new Vector2(400, 50), startScreen.transform);
        CreateText("Subtitle", "(вдохновение: Papers, please!)", new Vector2(0, 140), 16, Color.gray, new Vector2(400, 30), startScreen.transform);

        CreateText("AlgorithmTitle", "Алгоритм:", new Vector2(0, 100), 20, Color.yellow, new Vector2(400, 30), startScreen.transform);

        CreateText("Step1", "1. ГГ садится за стол.", new Vector2(0, 70), 14, Color.white, new Vector2(400, 25), startScreen.transform);
        CreateText("Step2", "2. Появляется интерфейс перебора карточек (одно окно, одна карточка за раз).", new Vector2(0, 45), 14, Color.white, new Vector2(500, 25), startScreen.transform);
        CreateText("Step2a", "   a. вверху «От кого» и иллюстрация", new Vector2(0, 20), 12, Color.gray, new Vector2(450, 20), startScreen.transform);
        CreateText("Step2b", "   b. тело письма", new Vector2(0, 0), 12, Color.gray, new Vector2(450, 20), startScreen.transform);
        CreateText("Step2c", "   c. под карточкой счет оставшихся писем", new Vector2(0, -20), 12, Color.gray, new Vector2(450, 20), startScreen.transform);
        CreateText("Step3", "3. Удалить свайп влево, принять: свайп вправо.", new Vector2(0, -50), 14, Color.white, new Vector2(450, 25), startScreen.transform);

        CreateText("Goal", "🎯 Цель: набрать больше 5 очков (нейтральные всегда правильные)", new Vector2(0, -90), 14, Color.green, new Vector2(500, 25), startScreen.transform);
        CreateText("Controls", "Управление: Q (удалить) | E (принять)", new Vector2(0, -120), 14, Color.cyan, new Vector2(400, 25), startScreen.transform);

        startButton = CreateButton("StartButton", "Начать работу (Пробел)", new Vector2(0, -160), new Color(0.2f, 0.8f, 0.2f), new Vector2(220, 50), startScreen.transform);
        startButton.onClick.AddListener(StartGame);

        Button startExitButton = CreateButton("StartExitButton", "Выход", new Vector2(0, -220), Color.gray, new Vector2(120, 40), startScreen.transform);
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

        CreateCardInterface();
        CreateGameButtons();
        CreateStatusTexts();
    }

    private void CreateCardInterface()
    {
        if (currentCardPrefab != null)
        {
            // Создаем экземпляр префаба
            currentCard = Instantiate(currentCardPrefab, gameScreen.transform);

            // Настраиваем позицию и размер
            RectTransform cardRect = currentCard.GetComponent<RectTransform>();
            cardRect.anchoredPosition = new Vector2(0, 20);
            cardRect.sizeDelta = cardSize;

            // Находим компоненты в префабе по структуре
            FindCardComponentsByPath();
        }
        else
        {
            Debug.LogError("CurrentCardPrefab не назначен в инспекторе!");
            CreateCardInterfaceOldWay();
        }
    }

    private void FindCardComponentsByPath()
    {
        // Ищем по точной структуре из скриншота
        Transform headerContainer = currentCard.transform.Find("HeaderContainer");
        if (headerContainer != null)
        {
            Transform headerText = headerContainer.Find("HeaderText");
            if (headerText != null)
                cardSenderText = headerText.GetComponent<TextMeshProUGUI>();
        }

        Transform contentContainer = currentCard.transform.Find("ContentContainer");
        if (contentContainer != null)
        {
            Transform contentText = contentContainer.Find("ContentText");
            if (contentText != null)
                cardContentText = contentText.GetComponent<TextMeshProUGUI>();
        }

        // Проверяем что нашли компоненты
        if (cardSenderText == null || cardContentText == null)
        {
            Debug.LogWarning("Не все Text компоненты найдены в префабе CurrentCard");
        }
        else
        {
            Debug.Log("✅ Компоненты карточки найдены в префабе");
        }
    }

    
    private void CreateCardInterfaceOldWay()
    {
        // Основная карточка в центре
        currentCard = new GameObject("CurrentCard");
        currentCard.transform.SetParent(gameScreen.transform, false);

        RectTransform cardRect = currentCard.AddComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.sizeDelta = cardSize;
        cardRect.anchoredPosition = new Vector2(0, 20);

        // Фон карточки
        Image cardBg = currentCard.AddComponent<Image>();
        cardBg.color = cardBackgroundColor;

        // Заголовок "От кого" с кружочком
        CreateCardHeader();

        // Основной контент карточки
        CreateCardContent();
    }

    private void CreateCardHeader()
    {
        GameObject headerContainer = new GameObject("HeaderContainer");
        headerContainer.transform.SetParent(currentCard.transform, false);

        RectTransform headerRect = headerContainer.AddComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0, 0.8f);
        headerRect.anchorMax = new Vector2(1, 1);
        headerRect.offsetMin = new Vector2(10, 0);
        headerRect.offsetMax = new Vector2(-10, -10);

        // Кружочек для аватара
        GameObject circle = new GameObject("SenderCircle");
        circle.transform.SetParent(headerContainer.transform, false);

        RectTransform circleRect = circle.AddComponent<RectTransform>();
        circleRect.anchorMin = new Vector2(0, 0.5f);
        circleRect.anchorMax = new Vector2(0, 0.5f);
        circleRect.sizeDelta = new Vector2(50, 50);
        circleRect.anchoredPosition = new Vector2(35, 0);

        Image circleImg = circle.AddComponent<Image>();
        circleImg.color = senderCircleColor;

        // Текст "От кого"
        GameObject senderTextObj = new GameObject("SenderText");
        senderTextObj.transform.SetParent(headerContainer.transform, false);

        cardSenderText = senderTextObj.AddComponent<TextMeshProUGUI>();
        cardSenderText.fontStyle = FontStyles.Bold;
        cardSenderText.color = cardTextColor;
        //cardSenderText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        //cardSenderText.fontSize = 18;

        //cardSenderText.fontStyle = FontStyle.Bold;
        cardSenderText.text = "От кого";

        RectTransform senderTextRect = senderTextObj.GetComponent<RectTransform>();
        senderTextRect.anchorMin = new Vector2(0.25f, 0);
        senderTextRect.anchorMax = new Vector2(1, 1);
        senderTextRect.offsetMin = Vector2.zero;
        senderTextRect.offsetMax = Vector2.zero;
    }

    private void CreateCardContent()
    {
        GameObject contentContainer = new GameObject("ContentContainer");
        contentContainer.transform.SetParent(currentCard.transform, false);

        RectTransform contentRect = contentContainer.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 0.1f);
        contentRect.anchorMax = new Vector2(1, 0.75f);
        contentRect.offsetMin = new Vector2(20, 0);
        contentRect.offsetMax = new Vector2(-20, 0);

        cardContentText = contentContainer.AddComponent<TextMeshProUGUI>();
      //  cardContentText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        cardContentText.fontSize = 16;
        cardContentText.color = cardTextColor;
       // cardContentText.alignment = TextAnchor.MiddleCenter;
        cardContentText.text = "Текст";
    }
    

    private void CreateStatusTexts()
    {
        // Счетчик карточек
        cardCounterText = CreateText("CardCounter", "Осталось: N", new Vector2(0, -200), 16, Color.white, new Vector2(200, 30), gameScreen.transform);

        // Счетчик очков  
        scoreText = CreateText("ScoreText", "Очки: 0", new Vector2(-200, 200), 18, Color.white, new Vector2(150, 30), gameScreen.transform);

        // Инструкции
        instructionText = CreateText("InstructionText", "Q - удалить ←  |  → принять - E", new Vector2(0, -250), 14, Color.yellow, new Vector2(400, 30), gameScreen.transform);
    }

    private void CreateGameButtons()
    {
        // Кнопка "Удалить" (красная)
        rejectButton = CreateButton("RejectButton", "Удалить (Q)", new Vector2(-120, -150), rejectButtonColor, new Vector2(100, 40), gameScreen.transform);
        rejectButton.onClick.AddListener(() => ProcessCard(false));

        // Кнопка "Принять" (зеленая)
        acceptButton = CreateButton("AcceptButton", "Принять (E)", new Vector2(120, -150), acceptButtonColor, new Vector2(100, 40), gameScreen.transform);
        acceptButton.onClick.AddListener(() => ProcessCard(true));

        // Кнопка выхода
        exitButton = CreateButton("ExitButton", "Выход", new Vector2(200, 200), Color.gray, new Vector2(80, 40), gameScreen.transform);
        exitButton.onClick.AddListener(ExitMiniGame);
    }

    protected override void StartGameLogic()
    {
        currentCardIndex = 0;
        correctAnswers = 0;
        incorrectAnswers = 0;
        isProcessingCard = false;
        cardsRemaining = gameCards.Count;

        UpdateUI();

        if (gameCards.Count > 0)
        {
            ShowCurrentCard();
        }
        else
        {
            Debug.LogError("Нет карточек для игры!");
            EndMiniGame();
        }
    }

    private void ShowCurrentCard()
    {
        if (currentCardIndex >= gameCards.Count)
        {
            CompleteGame();
            return;
        }

        CardData card = gameCards[currentCardIndex];


        // Обновляем содержимое карточки
        if (cardSenderText != null)
            cardSenderText.GetComponent<KnotLocalizedTextMeshProUGUI>().KeyReference.Key = card.sender;
        //cardSenderText.text = card.sender;

        if (cardContentText != null)
            cardContentText.GetComponent<KnotLocalizedTextMeshProUGUI>().KeyReference.Key = card.content;
        //cardContentText.text = card.content;

        UpdateUI();

        // Анимация появления карточки
        StartCoroutine(AnimateCardAppear());
    }

    private IEnumerator AnimateCardAppear()
    {
        if (currentCard == null) yield break;

        RectTransform cardRect = currentCard.GetComponent<RectTransform>();
        Vector2 targetPos = cardRect.anchoredPosition;
        Vector2 startPos = new Vector2(800f, targetPos.y); // Появляется справа

        cardRect.anchoredPosition = startPos;

        float elapsedTime = 0f;
        float duration = 0.3f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;

            cardRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, progress);
            yield return null;
        }

        cardRect.anchoredPosition = targetPos;
    }

    protected override void Update()
    {
        base.Update();

        if (!isGameActive || isProcessingCard) return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            ProcessCard(false); // Удалить
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            ProcessCard(true); // Принять
        }
    }

    protected override void OnActionButtonClick()
    {
        if (isGameActive && !isProcessingCard)
        {
            ProcessCard(true); // E = принять
        }
    }

    private void ProcessCard(bool accepted)
    {
        if (currentCardIndex >= gameCards.Count || isProcessingCard)
            return;

        isProcessingCard = true;

        CardData currentCardData = gameCards[currentCardIndex];

        // Проверяем правильность ответа
        bool isCorrect = (accepted && currentCardData.isWorkRelated) || (!accepted && !currentCardData.isWorkRelated);

        if (isCorrect)
        {
            correctAnswers++;
            Debug.Log($"✅ Правильно! {currentCardData.sender}: {currentCardData.content}");
        }
        else
        {
            incorrectAnswers++;
            Debug.Log($"❌ Неправильно! {currentCardData.sender}: {currentCardData.content}");
        }

        // Анимируем удаление карточки
        StartCoroutine(AnimateCardExit(accepted, isCorrect));
    }

    private IEnumerator AnimateCardExit(bool accepted, bool isCorrect)
    {
        if (currentCard == null) yield break;

        RectTransform cardRect = currentCard.GetComponent<RectTransform>();
        Vector2 startPos = cardRect.anchoredPosition;
        Vector2 targetPos = new Vector2(accepted ? 800f : -800f, startPos.y);

        // Цветовая обратная связь
        Image cardImg = currentCard.GetComponent<Image>();
        Color originalColor = cardImg.color;
        Color feedbackColor = isCorrect ? Color.green : Color.red;

        float elapsedTime = 0f;
        float duration = 0.4f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;

            cardRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, progress);
            cardRect.rotation = Quaternion.Lerp(Quaternion.identity,
                Quaternion.Euler(0, 0, accepted ? -20f : 20f), progress);

            // Мигающий эффект для обратной связи
            cardImg.color = Color.Lerp(originalColor, feedbackColor, Mathf.Sin(progress * Mathf.PI * 4));

            yield return null;
        }

        // Восстанавливаем карточку для следующего использования
        cardRect.anchoredPosition = Vector2.zero;
        cardRect.rotation = Quaternion.identity;
        cardImg.color = originalColor;

        currentCardIndex++;
        cardsRemaining--;
        isProcessingCard = false;

        yield return new WaitForSeconds(0.1f);

        ShowCurrentCard();
    }

    private void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = $"Очки: {correctAnswers}";

        if (cardCounterText != null)
            cardCounterText.text = $"Осталось: {cardsRemaining}";
    }

    private void CompleteGame()
    {
        isGameActive = false;

        // Создаем экран результатов
        CreateResultScreen();
    }

    private void CreateResultScreen()
    {
        resultScreen = new GameObject("ResultScreen");
        resultScreen.transform.SetParent(gameScreen.transform, false);

        RectTransform resultRect = resultScreen.AddComponent<RectTransform>();
        resultRect.anchorMin = Vector2.zero;
        resultRect.anchorMax = Vector2.one;
        resultRect.offsetMin = Vector2.zero;
        resultRect.offsetMax = Vector2.zero;

        // Фон результатов
        Image resultBg = resultScreen.AddComponent<Image>();
        resultBg.color = new Color(0, 0, 0, 0.9f);

        // Заголовок
        CreateText("ResultTitle", "Работа завершена", new Vector2(0, 100), 24, Color.white, new Vector2(400, 40), resultScreen.transform);

        // Статистика
        CreateText("CorrectCount", $"Правильно выбрано: {correctAnswers}", new Vector2(0, 50), 18, Color.green, new Vector2(300, 30), resultScreen.transform);
        CreateText("IncorrectCount", $"Неправильно выбрано: {incorrectAnswers}", new Vector2(0, 20), 18, Color.red, new Vector2(300, 30), resultScreen.transform);

        // Итоговый счет
        int finalScore = correctAnswers;
        CreateText("FinalScore", $"Счёт: {finalScore}", new Vector2(0, -20), 20, Color.yellow, new Vector2(200, 30), resultScreen.transform);

        // Результат
        bool victory = finalScore >= targetScore;
        string resultMessage = victory ? "Победа:" : "Проигрыш:";
        string resultDescription = victory ?
            "Игрок набрал больше 5 очков.\nКвест считается выполненным." :
            "Игрок отсортировал все карточки неправильно.\nКвест не выполнен.";

        CreateText("ResultMessage", resultMessage, new Vector2(0, -70), 18, victory ? Color.green : Color.red, new Vector2(200, 30), resultScreen.transform);
        CreateText("ResultDescription", resultDescription, new Vector2(0, -110), 14, Color.white, new Vector2(400, 50), resultScreen.transform);

        // Кнопка завершения
        Button finishButton = CreateButton("FinishButton", "Завершить", new Vector2(0, -180), Color.gray, new Vector2(120, 40), resultScreen.transform);
        finishButton.onClick.AddListener(EndMiniGame);

        // Вызываем событие результата
        OnGameAttempt?.Invoke(victory);

        // Автоматическое завершение через 5 секунд
        StartCoroutine(AutoFinish());
    }

    private IEnumerator AutoFinish()
    {
        yield return new WaitForSeconds(5f);
        if (resultScreen != null && resultScreen.activeInHierarchy)
        {
            EndMiniGame();
        }
    }

    protected override string CheckResult()
    {
        return correctAnswers >= targetScore ? "success" : "fail";
    }

    // Публичные методы для настройки игры
    public void SetCustomCards(List<CardData> customCards)
    {
        gameCards = new List<CardData>(customCards);
        if (gameCards.Count > maxCards)
        {
            gameCards = gameCards.GetRange(0, maxCards);
        }
        cardsRemaining = gameCards.Count;
        ShuffleCards();
    }

    public void AddCard(string sender, string content, bool isWorkRelated)
    {
        if (gameCards.Count < maxCards)
        {
            gameCards.Add(new CardData(sender, content, isWorkRelated));
            cardsRemaining = gameCards.Count;
        }
    }

    public void ClearCards()
    {
        gameCards.Clear();
        cardsRemaining = 0;
    }

    public void SetTargetScore(int score)
    {
        targetScore = score;
    }

    protected override void UpdateInstructionText(string message)
    {
        if (instructionText != null)
        {
            instructionText.text = message;
        }
    }
}