using Game.MiniGames;
using Knot.Localization.Components;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    [Header("UI References from Prefab")]
    [SerializeField] private GameObject uiPanel; // Весь UI панель из префаба

    [Header("Game Data")]
    [SerializeField] private List<CardData> gameCards = new List<CardData>();

    [Header("UI Prefabs")]
    [SerializeField] private GameObject currentCardPrefab; // Перетащите префаб в инспекторе

    // UI компоненты
    private GameObject currentCard;
    private TextMeshProUGUI cardSenderText;
    private TextMeshProUGUI cardContentText;
    private Text cardCounterText;
    private Text scoreText;
    private Button acceptButton;
    private Button rejectButton;
    private GameObject resultScreen;

    public int CardCount => gameCards.Count;

    // Состояние игры
    private int currentCardIndex = 0;
    private int correctAnswers = 0;
    private int incorrectAnswers = 0;
    private int cardsRemaining;
    private bool isProcessingCard = false;

    [Header("Card Stack Settings")]
    [SerializeField] private int visibleCardsInStack = 3; // Сколько карточек видно в стопке
    [SerializeField] private Vector2 stackOffset = new Vector2(5f, -5f); // Смещение каждой карточки
    [SerializeField] private float stackScaleReduction = 0.03f; // Уменьшение размера каждой карточки
    [SerializeField] private float stackAlphaReduction = 0.05f; // Уменьшение прозрачности
    [SerializeField] private float cardLockDuration = 1.5f; // Время блокировки карточки в секундах

    // UI компоненты для стопки
    private List<GameObject> cardStack = new List<GameObject>();
    private GameObject cardContainer; // Контейнер только для карточек

    private bool isCardLocked = false; // Заблокирована ли карточка
    private float cardLockTimer = 0f; // Таймер блокировки
    private bool _victory;
    
    public bool Victory => _victory;
    
    // Заменить метод CreateCardInterface():

    private void CreateCardInterface()
{
    if (currentCardPrefab != null)
    {
        // Создаем ОДИН экземпляр префаба со всем UI
        var baseCardPanel = Instantiate(currentCardPrefab, gameScreen.transform);
        var allObjects = baseCardPanel.GetComponentsInChildren<Transform>();
        
        // Находим карточку в префабе
        var cardTransform = allObjects.Where(obj => obj.name.Contains("CurrentCard")).FirstOrDefault();
        if (cardTransform != null)
        {
            // Создаем контейнер специально для стопки карточек
            cardContainer = new GameObject("CardStackContainer");
            cardContainer.transform.SetParent(gameScreen.transform, false);
            
            RectTransform containerRect = cardContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = Vector2.zero;
            containerRect.anchorMax = Vector2.one;
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;
            
            // Перемещаем оригинальную карточку в контейнер стопки
            cardTransform.SetParent(cardContainer.transform, false);
            
            CreateCardStack(cardTransform.gameObject);
        }
        else
        {
            Debug.LogError("CurrentCard не найдена в префабе!");
        }
    }
    else
    {
        Debug.LogError("CurrentCardPrefab не назначен в инспекторе!");
        CreateCardInterfaceOldWay();
    }
}

// Новый метод для создания стопки ТОЛЬКО карточек:

private void CreateCardStack(GameObject originalCard)
{
    cardStack.Clear();
    
    // Первая карточка - это оригинальная из префаба
    currentCard = originalCard;
    cardStack.Add(currentCard);
    
    // Создаем дополнительные карточки для стопки (копии только CurrentCard)
    for (int i = 1; i < visibleCardsInStack; i++)
    {
        // Создаем копию ТОЛЬКО карточки, не всего префаба
        GameObject duplicateCard = Instantiate(originalCard, cardContainer.transform);
        duplicateCard.name = $"CurrentCard_Stack_{i}";
        cardStack.Add(duplicateCard);
    }
    
    // Настраиваем позиции и стили для всех карточек в стопке
    SetupCardStackPositions();
    
    // Находим компоненты текста в активной (первой) карточке
    FindCardComponentsByPath();
    
    // Заполняем содержимое стопки
    UpdateStackContent();
    
    Debug.Log($"Создана стопка из {cardStack.Count} карточек");
}

// Метод для обновления содержимого стопки:

private void UpdateStackContent()
{
    for (int i = 0; i < cardStack.Count && i < visibleCardsInStack; i++)
    {
        int cardIndex = currentCardIndex + i;
        
        if (cardIndex < gameCards.Count)
        {
            // Есть карточка для отображения
            CardData cardData = gameCards[cardIndex];
            UpdateCardContent(cardStack[i], cardData);
            cardStack[i].SetActive(true);
        }
        else
        {
            // Нет больше карточек - скрываем
            cardStack[i].SetActive(false);
        }
    }
}

// Метод для обновления содержимого конкретной карточки:

private void UpdateCardContent(GameObject card, CardData cardData)
{
    // Находим компоненты текста в карточке
    Transform headerContainer = card.transform.Find("HeaderContainer");
    Transform contentContainer = card.transform.Find("ContentContainer");
    
    if (headerContainer != null)
    {
        Transform headerText = headerContainer.Find("HeaderText");
        if (headerText != null)
        {
            var senderText = headerText.GetComponent<TextMeshProUGUI>();
            if (senderText != null)
            {
                var localizedComponent = senderText.GetComponent<KnotLocalizedTextMeshProUGUI>();
                if (localizedComponent != null)
                    localizedComponent.KeyReference.Key = cardData.sender;
                else
                    senderText.text = cardData.sender;
            }
        }
    }
    
    if (contentContainer != null)
    {
        Transform contentText = contentContainer.Find("ContentText");
        if (contentText != null)
        {
            var contentTextComponent = contentText.GetComponent<TextMeshProUGUI>();
            if (contentTextComponent != null)
            {
                var localizedComponent = contentTextComponent.GetComponent<KnotLocalizedTextMeshProUGUI>();
                if (localizedComponent != null)
                    localizedComponent.KeyReference.Key = cardData.content;
                else
                    contentTextComponent.text = cardData.content;
            }
        }
    }
}

    // Заменить метод ShowCurrentCard():

    private void ShowCurrentCard()
    {
        if (currentCardIndex >= gameCards.Count)
        {
            CompleteGame();
            return;
        }

        // Обновляем содержимое всей стопки
        UpdateStackContent();
        UpdateUI();

        // БЛОКИРУЕМ карточку на указанное время
        StartCardLock();

        Debug.Log($"Показана карточка {currentCardIndex}, заблокирована на {cardLockDuration} сек");
    }

    // Обновить метод AnimateCardExit():

    private void SetupCardStackPositions()
    {
        Vector2 basePosition = new Vector2(0, 20);

        for (int i = 0; i < cardStack.Count; i++)
        {
            GameObject card = cardStack[i];
            RectTransform cardRect = card.GetComponent<RectTransform>();

            // Смещение для создания эффекта стопки (ИСПРАВЛЕНО: i вместо cardStack.Count - 1 - i)
            Vector2 offset = stackOffset * i; // Первая карточка (i=0) без смещения, последующие смещаются больше
            cardRect.anchoredPosition = basePosition + offset;

            // Размер (ИСПРАВЛЕНО: i вместо cardStack.Count - 1 - i)
            float scaleReduction = stackScaleReduction * i;
            Vector2 scaledSize = cardSize * (1f - scaleReduction);
            cardRect.sizeDelta = scaledSize;

            // Z-порядок (передняя карточка должна быть сверху) - ИСПРАВЛЕНО
            cardRect.SetSiblingIndex(cardStack.Count - 1 - i); // Первая карточка (i=0) будет иметь самый высокий индекс

            // Прозрачность (ИСПРАВЛЕНО: менее агрессивная прозрачность)
            CanvasGroup canvasGroup = card.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = card.AddComponent<CanvasGroup>();

            float alpha = 1f - (stackAlphaReduction * i); // Первая карточка полностью непрозрачная
            canvasGroup.alpha = Mathf.Max(alpha, 0.7f); // Минимальная прозрачность 70% вместо 30%

            // Блокируем взаимодействие с задними карточками
            if (i > 0)
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
            else
            {
                // Первая карточка всегда интерактивная
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }

            Debug.Log($"Карточка {i}: позиция {cardRect.anchoredPosition}, размер {scaledSize}, альфа {alpha}, sibling {cardRect.GetSiblingIndex()}");
        }
    }

    // Исправить метод AnimateCardExit() - правильная анимация удаления ВЕРХНЕЙ карточки:

    private IEnumerator AnimateCardExit(bool accepted, bool isCorrect)
    {
        if (currentCard == null || cardStack.Count == 0) yield break;

        // ВАЖНО: currentCard должна быть первой в списке (верхней)
        currentCard = cardStack[0];

        RectTransform cardRect = currentCard.GetComponent<RectTransform>();
        Vector2 startPos = cardRect.anchoredPosition;
        Vector2 targetPos = new Vector2(accepted ? 800f : -800f, startPos.y);

        // Цветовая обратная связь
        Image cardImg = currentCard.GetComponent<Image>();
        if (cardImg == null) cardImg = currentCard.GetComponentInChildren<Image>();

        Color originalColor = cardImg != null ? cardImg.color : Color.white;
        Color feedbackColor = isCorrect ? Color.green : Color.red;

        float elapsedTime = 0f;
        float duration = 0.4f;

        Debug.Log($"Анимация удаления карточки: {currentCard.name} (индекс 0 в стопке)");

        // Анимация удаления ТОЛЬКО верхней карточки
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;

            cardRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, progress);
            cardRect.rotation = Quaternion.Lerp(Quaternion.identity,
                Quaternion.Euler(0, 0, accepted ? -20f : 20f), progress);

            // Мигающий эффект для обратной связи
            if (cardImg != null)
                cardImg.color = Color.Lerp(originalColor, feedbackColor, Mathf.Sin(progress * Mathf.PI * 4));

            yield return null;
        }

        // ПРАВИЛЬНОЕ удаление верхней карточки
        GameObject removedCard = cardStack[0];
        cardStack.RemoveAt(0); // Удаляем первую (верхнюю) карточку

        // Перемещаем удаленную карточку в конец для переиспользования
        removedCard.transform.SetAsFirstSibling(); // Помещаем в самый конец по Z-order

        // Сбрасываем состояние удаленной карточки
        RectTransform removedRect = removedCard.GetComponent<RectTransform>();
        removedRect.anchoredPosition = new Vector2(0, 20);
        removedRect.rotation = Quaternion.identity;
        if (cardImg != null) cardImg.color = originalColor;

        // Скрываем удаленную карточку
        removedCard.SetActive(false);

        // Добавляем в конец стопки для будущего использования
        cardStack.Add(removedCard);

        Debug.Log($"Карточка удалена. Осталось в стопке: {cardStack.Count}");

        // Сдвигаем все оставшиеся карточки вперед
        yield return StartCoroutine(AnimateStackShift());

        currentCardIndex++;
        cardsRemaining--;
        isProcessingCard = false;

        yield return new WaitForSeconds(0.1f);

        ShowCurrentCard();
    }

    // Исправить метод AnimateStackShift():

    private IEnumerator AnimateStackShift()
    {
        float duration = 0.3f;
        float elapsedTime = 0f;

        // Запоминаем начальные позиции ТОЛЬКО для видимых карточек
        List<Vector2> startPositions = new List<Vector2>();
        List<Vector2> startSizes = new List<Vector2>();
        List<float> startAlphas = new List<float>();

        // Считаем только активные карточки (исключаем последнюю скрытую)
        int visibleCards = Mathf.Min(cardStack.Count - 1, visibleCardsInStack);

        for (int i = 0; i < visibleCards; i++)
        {
            RectTransform rect = cardStack[i].GetComponent<RectTransform>();
            CanvasGroup canvasGroup = cardStack[i].GetComponent<CanvasGroup>();

            startPositions.Add(rect.anchoredPosition);
            startSizes.Add(rect.sizeDelta);
            startAlphas.Add(canvasGroup != null ? canvasGroup.alpha : 1f);
        }

        // Вычисляем целевые позиции (каждая карточка сдвигается на позицию предыдущей)
        List<Vector2> targetPositions = new List<Vector2>();
        List<Vector2> targetSizes = new List<Vector2>();
        List<float> targetAlphas = new List<float>();

        Vector2 basePosition = new Vector2(0, 20);
        for (int i = 0; i < visibleCards; i++)
        {
            Vector2 offset = stackOffset * i; // Новая позиция для каждой карточки
            targetPositions.Add(basePosition + offset);

            float scaleReduction = stackScaleReduction * i;
            targetSizes.Add(cardSize * (1f - scaleReduction));

            float alpha = 1f - (stackAlphaReduction * i);
            targetAlphas.Add(Mathf.Max(alpha, 0.7f));
        }

        // Анимация сдвига
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;

            for (int i = 0; i < visibleCards; i++)
            {
                RectTransform rect = cardStack[i].GetComponent<RectTransform>();
                CanvasGroup canvasGroup = cardStack[i].GetComponent<CanvasGroup>();

                rect.anchoredPosition = Vector2.Lerp(startPositions[i], targetPositions[i], progress);
                rect.sizeDelta = Vector2.Lerp(startSizes[i], targetSizes[i], progress);

                if (canvasGroup != null)
                    canvasGroup.alpha = Mathf.Lerp(startAlphas[i], targetAlphas[i], progress);
            }

            yield return null;
        }

        // Устанавливаем финальные значения и правильные Z-order
        for (int i = 0; i < visibleCards; i++)
        {
            RectTransform rect = cardStack[i].GetComponent<RectTransform>();
            CanvasGroup canvasGroup = cardStack[i].GetComponent<CanvasGroup>();

            rect.anchoredPosition = targetPositions[i];
            rect.sizeDelta = targetSizes[i];
            rect.SetSiblingIndex(cardStack.Count - 1 - i); // Правильный Z-order

            if (canvasGroup != null)
            {
                canvasGroup.alpha = targetAlphas[i];
                canvasGroup.interactable = (i == 0); // Только первая карточка интерактивная
                canvasGroup.blocksRaycasts = (i == 0);
            }
        }

        // Обновляем текущую карточку (всегда первая в списке)
        if (cardStack.Count > 0)
        {
            currentCard = cardStack[0];
            FindCardComponentsByPath();
        }

        Debug.Log("Стопка сдвинута, новая верхняя карточка готова");
    }


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
            //InitializeDefaultCards();
        }

        cardsRemaining = gameCards.Count;
        base.Start();
    }

    protected void SetupPrefabButtons()
    {
        if (uiPanel != null)
        {
            acceptButton = uiPanel.transform.Find("AcceptButton")?.GetComponent<Button>();
            rejectButton = uiPanel.transform.Find("RejectButton")?.GetComponent<Button>();
            exitButton = uiPanel.transform.Find("ExitButton")?.GetComponent<Button>();

            // Подключаем обработчики
            if (acceptButton != null)
                acceptButton.onClick.AddListener(() => ProcessCard(true));
            if (rejectButton != null)
                rejectButton.onClick.AddListener(() => ProcessCard(false));
            if (exitButton != null)
                exitButton.onClick.AddListener(ExitMiniGame);
        }
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
            mainCanvas = FindObjectOfType<Canvas>();
            if (mainCanvas == null)
            {
                Debug.LogError("Canvas не найден в сцене!");
                return;
            }
        }

        miniGamePanel = GameObject.Find("WorkMiniGamePanel");
        if (miniGamePanel == null)
        {
            Debug.LogError("WorkMiniGamePanel не найдена!");
            return;
        }

        Debug.Log("=== ПОИСК КНОПОК ПО ВСЕЙ СЦЕНЕ ===");

        // Ищем ВСЕ кнопки в сцене
        Button[] allButtonsInScene = FindObjectsOfType<Button>();
        Debug.Log($"Всего кнопок в сцене: {allButtonsInScene.Length}");

        for (int i = 0; i < allButtonsInScene.Length; i++)
        {
            Button btn = allButtonsInScene[i];
            Debug.Log($"Кнопка {i}: '{btn.name}' в объекте '{btn.gameObject.name}' - активна: {btn.gameObject.activeInHierarchy}");
        }

        // Ищем конкретно ButtonQ и ButtonE
        GameObject buttonQObj = GameObject.Find("ButtonQ");
        GameObject buttonEObj = GameObject.Find("ButtonE");

        Debug.Log($"ButtonQ объект найден: {buttonQObj != null}");
        Debug.Log($"ButtonE объект найден: {buttonEObj != null}");

        if (buttonQObj != null)
        {
            rejectButton = buttonQObj.GetComponent<Button>();
            Debug.Log($"ButtonQ компонент Button: {rejectButton != null}");
        }

        if (buttonEObj != null)
        {
            acceptButton = buttonEObj.GetComponent<Button>();
            Debug.Log($"ButtonE компонент Button: {acceptButton != null}");
        }

        // Альтернативный поиск - по тексту на кнопках
        foreach (Button btn in allButtonsInScene)
        {
            Text btnText = btn.GetComponentInChildren<Text>();
            if (btnText != null)
            {
                Debug.Log($"Кнопка '{btn.name}' содержит текст: '{btnText.text}'");

                if (btnText.text.Contains("Q") && rejectButton == null)
                {
                    rejectButton = btn;
                    Debug.Log($"✅ Найдена кнопка Q по тексту: {btn.name}");
                }
                else if (btnText.text.Contains("E") && acceptButton == null)
                {
                    acceptButton = btn;
                    Debug.Log($"✅ Найдена кнопка E по тексту: {btn.name}");
                }
            }
        }

        Debug.Log($"ФИНАЛЬНЫЙ РЕЗУЛЬТАТ: rejectButton={rejectButton?.name}, acceptButton={acceptButton?.name}");
    }

    private void LogChildrenRecursive(Transform parent, int level)
    {
        string indent = new string(' ', level * 2);
        Debug.Log($"{indent}{parent.name} ({parent.GetType().Name})");

        for (int i = 0; i < parent.childCount; i++)
        {
            LogChildrenRecursive(parent.GetChild(i), level + 1);
        }
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

        startButton = CreateButton("StartButton", "Начать работу (Пробел)", new Vector2(-200, 0), new Color(0.2f, 0.8f, 0.2f), new Vector2(300, 100), startScreen.transform, 24);
        startButton.onClick.AddListener(StartGame);

        Button startExitButton = CreateButton("StartExitButton", "Выход", new Vector2(200, 0), Color.gray, new Vector2(300, 100), startScreen.transform, 24);
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
        SetupPrefabButtons();
        CreateGameButtons();
        CreateStatusTexts();
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

        exitButton = CreateButton("ExitButton", "Выход", new Vector2(200, 200), Color.gray, new Vector2(80, 40), gameScreen.transform);
        exitButton.onClick.AddListener(ExitMiniGame);
    }

    protected override void StartGameLogic()
    {
        currentCardIndex = 0;
        correctAnswers = 0;
        incorrectAnswers = 0;
        isProcessingCard = false;
        isCardLocked = false; // Сбрасываем блокировку
        cardLockTimer = 0f; // Сбрасываем таймер
        cardsRemaining = gameCards.Count;

        UpdateUI();

        if (gameCards.Count > 0)
        {
            ShowCurrentCard(); // Это заблокирует первую карточку автоматически
        }
        else
        {
            Debug.LogError("Нет карточек для игры!");
            EndMiniGame();
        }
    }

    private void StartCardLock()
    {
        isCardLocked = true;
        cardLockTimer = cardLockDuration;

        // Визуальная индикация блокировки
        UpdateInstructionText($"Подождите {cardLockDuration:F1} сек...");

        StartCoroutine(CardLockCountdown());
    }

    // Корутина для отсчета времени блокировки:
    private IEnumerator CardLockCountdown()
    {
        while (cardLockTimer > 0f)
        {
            cardLockTimer -= Time.deltaTime;

            // Обновляем текст с оставшимся временем
            if (cardLockTimer > 0f)
                UpdateInstructionText($"Подождите {cardLockTimer:F1} сек...");

            yield return null;
        }

        // Разблокируем карточку
        isCardLocked = false;
        UpdateInstructionText("Q - удалить ←  |  → принять - E");

        Debug.Log("Карточка разблокирована!");
    }

    // Обновить методы ввода с проверкой блокировки:

    protected override void Update()
    {
        base.Update();

        if (isGameActive && !isProcessingCard && !isCardLocked) // Проверка блокировки
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                ProcessCard(false); // Удалить
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                ProcessCard(true); // Принять
            }
        }
        else if (isCardLocked && (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.E)))
        {
            // Игрок пытается нажать кнопку во время блокировки
            Debug.Log($"Карточка заблокирована! Осталось: {cardLockTimer:F1} сек");
        }
    }

    protected override void QInput()
    {
        if (isGameActive && !isProcessingCard && !isCardLocked) // Проверка блокировки
        {
            ProcessCard(false); // Q = удалить
        }
        else if (isCardLocked)
        {
            Debug.Log($"Карточка заблокирована! Осталось: {cardLockTimer:F1} сек");
        }
    }

    protected override void OnActionButtonClick()
    {
        if (isGameActive && !isProcessingCard && !isCardLocked) // Проверка блокировки
        {
            ProcessCard(true); // E = принять
        }
        else if (isCardLocked)
        {
            Debug.Log($"Карточка заблокирована! Осталось: {cardLockTimer:F1} сек");
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
            model.Score += 100;
            correctAnswers++;
            Debug.Log($"✅ Правильно! {currentCardData.sender}: {currentCardData.content}");
        }
        else
        {
            model.Score -= 50;
            incorrectAnswers++;
            Debug.Log($"❌ Неправильно! {currentCardData.sender}: {currentCardData.content}");
        }

        // Анимируем удаление карточки
        StartCoroutine(AnimateCardExit(accepted, isCorrect));
    }

    /*
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
    */

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
        _victory = finalScore >= targetScore;
        bool victory = _victory;
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