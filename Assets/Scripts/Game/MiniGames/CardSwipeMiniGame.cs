using Game.MiniGames;
using Knot.Localization.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.WSA;
using Utilities;
using static CardSwipeMiniGame;
using Random = System.Random;

public enum IconMappingType
{
    None = 0,
    Editor,
    Maria,
    Client,
    Bank,
    Prince,
    Word,
    Kira,
    Mom,
    Coach,
    Unknown,
    Sophia,
    Lifeline,
    Ex,
    Medical,
    Patient,
    Uncle,
    Format,
    drVera,
    Sasha,
    Survey,
    Colleague,
}

[Serializable]
internal struct IconMapping
{
    [FormerlySerializedAs("type")] public IconMappingType mappingType;
    public Sprite sprite;
}

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
    [SerializeField] private BaseCardPanel uiPanel; // Весь UI панель из префаба

    [Header("Game Data")]
    [SerializeField] private List<CardData> gameCards = new List<CardData>();

    public List<CardData> GameCards => gameCards;

    [Header("UI Prefabs")]
    [SerializeField] private GameObject currentCardPrefab; // Перетащите префаб в инспекторе


    [Header("Card Typing Effect")]
    [SerializeField] private bool enableTypingEffect = true;
    [SerializeField] private float headerTypingSpeed = 0.08f;
    [SerializeField] private float contentTypingSpeed = 0.04f;

    [SerializeField] private List<IconMapping> mappings;
    private Dictionary<IconMappingType, Sprite> _iconMappingDict;

    // UI компоненты
    private GameObject currentCard;
    private TextMeshProUGUI cardSenderText;
    private TextMeshProUGUI cardContentText;
    private TextMeshProUGUI cardCounterText;
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
    //[SerializeField] private float stackAlphaReduction = 0.05f; // Уменьшение прозрачности
    [SerializeField] private float cardLockDuration = 1.5f; // Время блокировки карточки в секундах


    [Header("Feedback Icons")]
    [SerializeField] private float feedbackIconDuration = 1.0f; // Время показа иконки
    [SerializeField] private Color successColor = Color.green;
    [SerializeField] private Color failureColor = Color.red;
    [SerializeField] private Color defaulColor = Color.gray;

    [Header("Last Card Animation")]
    [SerializeField] private float lastCardScale = 2.5f;
    [SerializeField] private float lastCardAnimationDuration = 1.0f;
    [SerializeField] private string lastCardSuccessButtonText = "ЗАВЕРШИТЬ"; // Новый текст для кнопки

    [Header("Last Card Shake Settings")]
    [SerializeField] private float shakeIntensity = 10f; // Сила тряски
    [SerializeField] private float shakeSpeed = 15f; // Скорость тряски
    [SerializeField] private bool enableShake = true; // Включить/выключить тряску

    private bool isLastCard = false;
    private Coroutine lastCardAnimationCoroutine;
    private Coroutine continuousShakeCoroutine;

    // UI компоненты для стопки
    private List<GameObject> cardStack = new List<GameObject>();
    private GameObject cardContainer; // Контейнер только для карточек

    private bool isCardLocked = false; // Заблокирована ли карточка
    private float cardLockTimer = 0f; // Таймер блокировки
    private bool _victory;

    public bool Victory => _victory;

    private bool firstCard = true;

    // Заменить метод CreateCardInterface():


    private void Awake()
    {
        // Инициализируем словарь для быстрого доступаvar dict1 = new Dictionary<MyKeyType, Sprite>();
        var dict1 = new Dictionary<IconMappingType, Sprite>();
        foreach (var m in mappings)
        {
            dict1[m.mappingType] = m.sprite;
        }
        _iconMappingDict = dict1;
    }

    private string GetTextFromCardData(TextMeshProUGUI textComponent, string key)
    {
        var localizedComponent = textComponent.GetComponent<KnotLocalizedTextMeshProUGUI>();
        if (localizedComponent != null)
        {
            localizedComponent.KeyReference.Key = key;
            return localizedComponent.KeyReference.Value; // или получить текст другим способом
        }
        return key;
    }

    private Transform FindChildRecursive(Transform parent, string name)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name == name) return child;

            Transform found = FindChildRecursive(child, name);
            if (found != null) return found;
        }
        return null;
    }


    private void CreateCardInterface()
    {
        if (currentCardPrefab != null)
        {
            // Создаем ОДИН экземпляр префаба со всем UI
            var baseCardPanel = Instantiate(currentCardPrefab, gameScreen.transform);

            var allObjects = baseCardPanel.GetComponentsInChildren<Transform>();

            currentCardPrefab = baseCardPanel.gameObject;
            //uiPanel.gameObject. = currentCardPrefab;
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
        //UpdateStackContent();

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

                //UpdateCardIcon(i, cardStack[i]);
                if (currentCardIndex < 3)
                {
                    var ch = cardStack[i].GetComponentsInChildren<Image>().Where(ch => ch.name == "SenderImage");
                    if (ch.Count() > 0)
                    {
                        var senderImage = ch.First();
                        ch.First().sprite = _iconMappingDict[cardData.IconCardType];
                    }
                }

                if (firstCard)
                {


                    // Передняя карточка - показываем текст с анимацией
                    UpdateCardContent(cardStack[i], cardData);
                    firstCard = false;
                }
                else
                {
                    // Задние карточки - скрываем текст
                    HideCardText(cardStack[i]);
                    // Но сохраняем данные для будущего показа
                    StoreCardData(cardStack[i], cardData);
                }

                cardStack[i].SetActive(true);
            }
            else
            {
                // Нет больше карточек - скрываем
                cardStack[i].SetActive(false);
            }
        }
    }

    private void HideCardText(GameObject card)
    {
        // Ищем все TextMeshProUGUI компоненты в карточке
        TextMeshProUGUI[] allTexts = card.GetComponentsInChildren<TextMeshProUGUI>();
        //UpdateCardIcon(currentCardIndex, card);


        foreach (var text in allTexts)
        {
            text.text = ""; // Очищаем текст

            // Или делаем прозрачным
            // text.color = new Color(text.color.r, text.color.g, text.color.b, 0f);
        }

        Debug.Log($"Текст скрыт на карточке: {card.name}");
    }

    private void StoreCardData(GameObject card, CardData cardData)
    {
        // Сохраняем данные в компоненте для будущего использования
        CardDataHolder holder = card.GetComponent<CardDataHolder>();
        if (holder == null)
            holder = card.AddComponent<CardDataHolder>();


        holder.cardData = cardData;
    }

    // Вспомогательный компонент для хранения данных карточки
    public class CardDataHolder : MonoBehaviour
    {
        public CardData cardData;
    }

    // Метод для обновления содержимого конкретной карточки:

    private void UpdateCardContent(GameObject card, CardData cardData)
    {

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

                    string finalText = GetTextFromCardData(senderText, cardData.sender);
                    senderText.text = "";


                    senderText.text = finalText;
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
                    contentTextComponent.text = "";
                    string finalText = GetTextFromCardData(contentTextComponent, cardData.content);

                    // if (enableTypingEffect)
                    //   StartCoroutine(TypeText(contentTextComponent, finalText, contentTypingSpeed, 0.3f));
                    //else
                    contentTextComponent.text = finalText;

                }
            }
        }
        StartCoroutine(lockText());

    }

    IEnumerator lockText()
    {
        var textFader = currentCard?.GetComponent<SequentialTextFader>();
        float waitTime = textFader?.TotalAnimationDuration ?? (feedbackIconDuration - 0.6f);

        yield return new WaitForSeconds(waitTime);
        isCardLocked = false;           // 1. Разблокируем карточку
        RestoreIconColors();

        
    }

    private void ShowCurrentCard()
    {
        if (currentCardIndex >= gameCards.Count)
        {
            CompleteGame();
            return;
        }

        // Проверяем, является ли это последней карточкой
        isLastCard = (currentCardIndex == gameCards.Count - 1); //&& (MiniGameCoordinator.DayLevel == 1);

        if (firstCard)
        { 
            StartCoroutine(ForceButtonPress(acceptButton));  // E кнопка выглядит нажатой
            StartCoroutine(ForceButtonPress(rejectButton));  // Q кнопка выглядит нажатой
        }

    
           
        // Обновляем содержимое всей стопки
        UpdateStackContent();
        UpdateUI();

        // Если это последняя карточка - запускаем специальную анимацию
        if (isLastCard)
        {
            
            if (MiniGameCoordinator.DayLevel == 1)
                StartLastCardAnimation();
        }

        Debug.Log($"Показана карточка {currentCardIndex}, заблокирована на {cardLockDuration} сек");
    }


    private void StartLastCardAnimation()
    {
        if (lastCardAnimationCoroutine != null)
        {
            StopCoroutine(lastCardAnimationCoroutine);
        }

        // Останавливаем предыдущую тряску если есть
        if (continuousShakeCoroutine != null)
        {
            StopCoroutine(continuousShakeCoroutine);
        }

        lastCardAnimationCoroutine = StartCoroutine(AnimateLastCard());
    }

    private IEnumerator AnimateLastCard()
    {
        if (currentCard == null) yield break;

        // Находим кнопку success для изменения текста
        UpdateSuccessButtonText(lastCardSuccessButtonText);

        // Получаем RectTransform карточки
        RectTransform cardRect = currentCard.GetComponent<RectTransform>();
        Vector3 originalScale = cardRect.localScale;
        Vector3 targetScale = originalScale * lastCardScale;
        Vector2 originalPosition = cardRect.anchoredPosition;

        float elapsedTime = 0f;

        // Анимация увеличения с тряской
        while (elapsedTime < lastCardAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / lastCardAnimationDuration;

            // Используем easing для плавной анимации scale
            float easedProgress = EaseInOutQuad(progress);
            cardRect.localScale = Vector3.Lerp(originalScale, targetScale, easedProgress);

            // Добавляем shake эффект
            if (enableShake)
            {
                Vector2 shakeOffset = CalculateShakeOffset(elapsedTime);
                cardRect.anchoredPosition = originalPosition + shakeOffset;
            }

            yield return null;
        }

        // Устанавливаем финальные значения
        cardRect.localScale = targetScale;
        cardRect.anchoredPosition = originalPosition; // Возвращаем к исходной позиции

        // Продолжаем тряску после завершения увеличения
        if (enableShake)
        {
            StartCoroutine(ContinuousShake(cardRect, originalPosition));
        }

        Debug.Log("🎉 Анимация последней карточки завершена!");
    }

    private void UpdateSuccessButtonText(string newText)
    {
        var go = currentCardPrefab.gameObject;
        var allButtons = go.GetComponentsInChildren<UnityEngine.UI.Button>().ToList();

        // Ищем кнопку success (обычно это acceptButton)
        if (acceptButton != null)
        {
            var buttonText = acceptButton.GetComponentInChildren<UnityEngine.UI.Text>();
            if (buttonText != null)
            {
                buttonText.text = newText;
                Debug.Log($"Текст кнопки изменен на: {newText}");
            }

            // Если используется TextMeshPro
            var buttonTextTMP = acceptButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (buttonTextTMP != null)
            {
                buttonTextTMP.text = newText;
                Debug.Log($"Текст кнопки TMP изменен на: {newText}");
            }
        }

        // Альтернативный поиск кнопки success по Image с именем "success"
        var imgs = go.GetComponentsInChildren<UnityEngine.UI.Image>().ToList();
        var successImage = imgs.Where(img => img.name.ToLower().Contains("success")).FirstOrDefault();

        if (successImage != null)
        {
            var parentButton = successImage.GetComponentInParent<UnityEngine.UI.Button>();
            if (parentButton != null)
            {
                var buttonText = parentButton.GetComponentInChildren<UnityEngine.UI.Text>();
                if (buttonText != null)
                {
                    buttonText.text = newText;
                }

                var buttonTextTMP = parentButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (buttonTextTMP != null)
                {
                    buttonTextTMP.text = newText;
                }
            }
        }
    }


    // Новый метод для расчета shake offset
    private Vector2 CalculateShakeOffset(float time)
    {
        float shakeX = Mathf.Sin(time * shakeSpeed) * shakeIntensity;
        float shakeY = Mathf.Cos(time * shakeSpeed * 1.1f) * shakeIntensity * 0.5f; // Меньше тряски по Y

        return new Vector2(shakeX, shakeY);
    }

    // Непрерывная тряска после увеличения
    private IEnumerator ContinuousShake(RectTransform cardRect, Vector2 originalPosition)
    {
        float shakeTime = 0f;

        while (isLastCard && currentCard != null && cardRect != null)
        {
            shakeTime += Time.deltaTime;

            Vector2 shakeOffset = CalculateShakeOffset(shakeTime);
            cardRect.anchoredPosition = originalPosition + shakeOffset;

            yield return null;
        }

        // Возвращаем к исходной позиции при завершении
        if (cardRect != null)
        {
            cardRect.anchoredPosition = originalPosition;
        }
    }

    // Easing функция для плавной анимации
    private float EaseInOutQuad(float t)
    {
        return t < 0.5f ? 2f * t * t : -1f + (4f - 2f * t) * t;
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


            // float alpha = 1f - (stackAlphaReduction * i); // Первая карточка полностью непрозрачная
            canvasGroup.alpha = 1;// Mathf.Max(alpha, 0.7f); // Минимальная прозрачность 70% вместо 30%

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

            // Debug.Log($"Карточка {i}: позиция {cardRect.anchoredPosition}, размер {scaledSize}, альфа {alpha}, sibling {cardRect.GetSiblingIndex()}");
        }
    }

    private IEnumerator AnimateCardExit(bool accepted, bool isCorrect)
    {
        if (currentCard == null || cardStack.Count == 0) yield break;


        if (isLastCard)
        {
            if (lastCardAnimationCoroutine != null)
            {
                StopCoroutine(lastCardAnimationCoroutine);
            }

            if (continuousShakeCoroutine != null)
            {
                StopCoroutine(continuousShakeCoroutine);
            }

            // Сбрасываем scale и позицию карточки перед анимацией выхода
            RectTransform _cardRect = currentCard.GetComponent<RectTransform>();
            _cardRect.localScale = Vector3.one;
            _cardRect.anchoredPosition = new Vector2(0, 20); // Исходная позиция

            // Возвращаем оригинальный текст кнопки
            UpdateSuccessButtonText("E"); // или оригинальный текст
        }
        // ВАЖНО: currentCard должна быть первой в списке (верхней)
        currentCard = cardStack[0];

        // if(cardStack.Count > 1)
        //   UpdateCardIcon(currentCardIndex, cardStack[1]);

        RectTransform cardRect = currentCard.GetComponent<RectTransform>();
        Vector2 startPos = cardRect.anchoredPosition;
        Vector2 targetPos = new Vector2(accepted ? 800f : -800f, startPos.y);

        // Находим иконки success и decay
        var go = currentCardPrefab.gameObject;
        var imgs = go.GetComponentsInChildren<Image>().ToList();

        var successIcon = imgs.Where(ch => ch.name.Contains("success")).FirstOrDefault();
        var declayIcon = imgs.Where(ch => ch.name.Contains("decay")).FirstOrDefault();


        StartCoroutine(ShowFeedbackIcon(successIcon, /*successColor*/ defaulColor));

        StartCoroutine(ShowFeedbackIcon(declayIcon,/* failureColor*/ defaulColor));

        // Цветовая обратная связь для карточки (опционально)
        Image cardImg = currentCard.GetComponent<Image>();
        if (cardImg == null) cardImg = currentCard.GetComponentInChildren<Image>();

        Color originalColor = cardImg != null ? cardImg.color : Color.white;

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

    private IEnumerator ShowFeedbackIcon(Image icon, Color feedbackColor)
    {
        if (icon == null) yield break;

        // Сохраняем оригинальные значения
        Color originalColor = icon.color;
        Vector3 originalScale = icon.transform.localScale;
        bool wasActive = icon.gameObject.activeInHierarchy;

        // Активируем иконку и устанавливаем цвет
        icon.gameObject.SetActive(true);
        icon.color = feedbackColor;

        // Анимация появления (масштабирование)
        float appearDuration = 0.2f;
        float elapsedTime = 0f;

        // Начинаем с нулевого масштаба
        icon.transform.localScale = Vector3.zero;

        while (elapsedTime < appearDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / appearDuration;

            // Эффект "pop" - слегка превышаем целевой размер, затем возвращаемся
            float scale = Mathf.LerpUnclamped(0f, 1.2f, progress);
            if (progress > 0.7f)
            {
                scale = Mathf.Lerp(1.2f, 1f, (progress - 0.7f) / 0.3f);
            }

            icon.transform.localScale = originalScale * scale;
            yield return null;
        }
        icon.transform.localScale = originalScale;
        // ИЗМЕНИТЬ ЭТУ ЧАСТЬ - использовать время анимации текста:
        var textFader = currentCard?.GetComponent<SequentialTextFader>();
        float holdTime = textFader?.TotalAnimationDuration ?? feedbackIconDuration;
        holdTime -= appearDuration * 2; // Вычитаем время появления и исчезновения

        // Устанавливаем финальный размер


        // Держим иконку видимой
        // float holdTime = feedbackIconDuration - appearDuration * 2; // Вычитаем время появления и исчезновения
        if (holdTime > 0)
        {
            yield return new WaitForSeconds(holdTime);
        }

        // Анимация исчезновения (плавное затухание)
        float fadeDuration = appearDuration;
        elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / fadeDuration;

            // Плавное уменьшение альфа-канала
            Color currentColor = icon.color;
            currentColor.a = Mathf.Lerp(currentColor.a, 0f, progress);
            icon.color = currentColor;

            // Легкое уменьшение масштаба
            float scale = Mathf.Lerp(1f, 0.8f, progress);
            icon.transform.localScale = originalScale * scale;

            yield return null;
        }

        // Восстанавливаем оригинальные значения
        icon.color = originalColor;
        icon.transform.localScale = originalScale;
        icon.gameObject.SetActive(wasActive);
    }
    // Исправить метод AnimateStackShift():

    private IEnumerator AnimateStackShift()
    {
        float duration = 0.3f;
        float elapsedTime = 0f;


        if (cardStack.Count > 1)
        {
            CardDataHolder holder = cardStack[1].GetComponent<CardDataHolder>();
            if (holder != null && holder.cardData != null)
            {
                var ch = cardStack[1].GetComponentsInChildren<Image>().Where(ch => ch.name == "SenderImage");
                if (ch.Count() > 0)
                {
                    var senderImage = ch.First();
                    ch.First().sprite = _iconMappingDict[holder.cardData.IconCardType];
                }
            }
        }



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

            //float alpha = 1f - (stackAlphaReduction * i);
            //targetAlphas.Add(Mathf.Max(alpha, 0.7f));
        }

        var go = currentCardPrefab.gameObject;
        var imgs = go.GetComponentsInChildren<Image>().ToList();


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


                //var getimg = img.GetComponentsInChildren<Image>();
                //if (canvasGroup != null)
                // canvasGroup.alpha = Mathf.Lerp(startAlphas[i], targetAlphas[i], progress);
            }

            yield return null;
        }

        if (cardStack.Count > 0)
        {
            currentCard = cardStack[0];
            FindCardComponentsByPath();
            CardDataHolder holder = currentCard.GetComponent<CardDataHolder>();
            // Получаем данные для новой передней карточки
            if (holder != null && holder.cardData != null)
            {

                // Запускаем анимацию печатания на новой передней карточке
                StartCoroutine(DelayedCardReveal(currentCard, holder.cardData));
            }
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
                //canvasGroup.alpha = targetAlphas[i];
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

    private IEnumerator DelayedCardReveal(GameObject card, CardData cardData)
    {
        // Небольшая задержка перед началом печатания
        yield return new WaitForSeconds(0.2f);

        // Показываем текст с анимацией печатания
        UpdateCardContent(card, cardData);
        var textFader = card.GetComponent<SequentialTextFader>();
        if (textFader != null)
        {
            textFader.enabled = false; // Сбрасываем
            yield return new WaitForEndOfFrame();
            textFader.enabled = true;  // Перезапускаем анимацию
        }
    }

    public enum MessageType : ushort
    {
        WORK,
        SPAM,
        FREND
    }


    [System.Serializable]
    public class CardData
    {
        [Header("Card Header")]
        public string sender; // "От кого"

        [Header("Card Content")]
        public string content; // Основной текст

        [Header("Card Type")]
        public MessageType TypeCard; // true = принять (работа), false = удалить (личное)

        [Header("Icon Type")]
        public IconMappingType IconCardType;

        public CardData(string sender, string content, int workRelated, IconMappingType icon = IconMappingType.None)
        {
            this.sender = sender;
            this.content = content;
            TypeCard = (MessageType)workRelated;
            IconCardType = icon;
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

            var panal = currentCardPrefab.gameObject;
            acceptButton = panal.GetComponent("ButtonE") as Button;
            //acceptButton = panal.transform.Find()?.GetComponent<Button>();
            rejectButton = panal.GetComponent("ButtonQ") as Button;
            // exitButton = panal.transform.Find("ExitButton")?.GetComponent<Button>();
            var _baseButton = panal.GetComponentsInChildren<Button>().ToList();

            acceptButton = _baseButton[1];
            rejectButton = _baseButton[0];
            //  exitButton = uiPanel.transform.Find("ExitButton")?.GetComponent<Button>();



        }
    }


    private void ShuffleCards()
    {
        for (int i = 0; i < gameCards.Count; i++)
        {
            CardData temp = gameCards[i];
            int randomIndex = UnityEngine.Random.Range(i, gameCards.Count);
            gameCards[i] = gameCards[randomIndex];
            gameCards[randomIndex] = temp;
        }
        if (MiniGameCoordinator.DayLevel == 1)
        {

            for (int i = 0; i < gameCards.Count; i++)
            {
                if (gameCards[i].sender == "Day2_CardHeader9")
                {
                    var temp = gameCards[i];
                    gameCards[i] = gameCards[gameCards.Count - 1];
                    gameCards[gameCards.Count - 1] = temp;


                }
            }

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

        //Button startExitButton = CreateButton("StartExitButton", "Выход", new Vector2(200, 0), Color.gray, new Vector2(300, 100), startScreen.transform, 24);
        //startExitButton.onClick.AddListener(ExitMiniGame);
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
        //CreateGameButtons();
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
        //var images = headerContainer.GetComponentsInChildren<Image>();
        // images.First().color =  ;


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
        var txt = currentCardPrefab.transform.Find("Score");
        // txt.faceColor
        var component = txt.GetComponent<TextMeshProUGUI>();

        cardCounterText = component; //CreateText("CardCounter", "Осталось: N", new Vector2(0, -300), 16, Color.white, new Vector2(200, 30), gameScreen.transform);
        txt.position = new Vector3(txt.position.x, txt.position.y - 150, txt.position.z);
        // Счетчик очков  
        //scoreText = CreateText("ScoreText", "Очки: 0", new Vector2(-200, 200), 18, Color.white, new Vector2(150, 30), gameScreen.transform);

        // Инструкции
        instructionText = CreateText("InstructionText", "Q - удалить ←  |  → принять - E", new Vector2(0, -250), 14, Color.yellow, new Vector2(400, 30), gameScreen.transform);
        instructionText.gameObject.SetActive(false);
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

            StartCardLock();
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

        //  StartCoroutine(CardLockCountdown());
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
        if (isGameActive && !isProcessingCard && !isCardLocked)
        {

            StartCoroutine(ForceButtonPress(rejectButton));
        }
        else if (isCardLocked)
        {
            Debug.Log($"Карточка заблокирована! Осталось: {cardLockTimer:F1} сек");
        }
    }



    protected override void OnActionButtonClick()
    {
        if (isGameActive && !isProcessingCard && !isCardLocked)
        {
            StartCoroutine(ForceButtonPress(acceptButton));
        }
        else if (isCardLocked)
        {
            Debug.Log($"Карточка заблокирована! Осталось: {cardLockTimer:F1} сек");
        }
    }

    private IEnumerator ForceButtonPress(Button button)
    {
        if (button == null) yield break;

        //uiPanel.gameObject
        // Принудительно переводим кнопку в состояние Pressed
        button.targetGraphic.CrossFadeColor(button.colors.pressedColor, 0f, true, true);

        // Или если используем Sprite Swap, меняем спрайт напрямую
        Image buttonImage = button.targetGraphic as Image;
        if (buttonImage != null && button.spriteState.pressedSprite != null)
        {
            Sprite originalSprite = buttonImage.sprite;
            buttonImage.sprite = button.spriteState.pressedSprite;

            yield return new WaitForSeconds(cardLockDuration);

            buttonImage.sprite = originalSprite;
        }
        else
        {
            // Fallback для Color Tint
            yield return new WaitForSeconds(cardLockDuration);
            button.targetGraphic.CrossFadeColor(button.colors.normalColor, 0f, true, true);
        }

        // Вызываем клик
        button.onClick.Invoke();
    }

    private void ProcessCard(bool accepted)
    {
        if (currentCardIndex >= gameCards.Count || isProcessingCard)
            return;

        isProcessingCard = true;

        CardData currentCardData = gameCards[currentCardIndex];

        // Проверяем правильность ответа
        bool isCorrect =
            ((accepted && currentCardData.TypeCard == MessageType.WORK)
            || (!accepted && (currentCardData.TypeCard == MessageType.SPAM)
            || currentCardData.TypeCard == MessageType.FREND));

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
        StartCardLock();
        // Анимируем удаление карточки
        StartCoroutine(AnimateCardExit(accepted, isCorrect));
    }
    private void UpdateUI()
    {
        //if (scoreText != null)
        // scoreText.text = $"Очки: {correctAnswers}";

        if (cardCounterText != null)
        {
            var _cardCount = CardCount - cardsRemaining;
            cardCounterText.text = $"{_cardCount}/{CardCount} ";
            //cardCounterText.font = cardSenderText.font.sourceFontFile;
        }
    }

    
    IEnumerator HideElement()
    {
        yield return new WaitForEndOfFrame();
        var go = currentCardPrefab.gameObject;
        var imgs = go.GetComponentsInChildren<UnityEngine.UI.Image>().ToList();
        var successIcon = imgs.Where(ch => ch.name.Contains("success")).FirstOrDefault();
        var declayIcon = imgs.Where(ch => ch.name.Contains("decay")).FirstOrDefault();

        // Запускаем анимации hide (если есть UIElementTweener)
        var successTweener = successIcon?.GetComponent<UIElementTweener>();
        var declayTweener = declayIcon?.GetComponent<UIElementTweener>();
        var acceptTweener = acceptButton?.GetComponent<UIElementTweener>();
        var rejectTweener = rejectButton?.GetComponent<UIElementTweener>();

        // Запускаем анимации hide
        successTweener?.Hide();
        declayTweener?.Hide();
        acceptTweener?.Hide();
        rejectTweener?.Hide();

        // Ждем завершения самой длинной анимации


        yield return new WaitForSeconds(0.5f);
    }

    private void CompleteGame()
    {
        StartCoroutine(HideElement());
        isGameActive = false;

        int finalScore = correctAnswers;
        // Результат
        _victory = finalScore >= targetScore;
        bool victory = _victory;

        OnGameAttempt?.Invoke(victory);


        StartCoroutine(AutoFinish());
        // Создаем экран результатов
    }


    private IEnumerator AutoFinish()
    {
        yield return new WaitForSeconds(0.1f);
        EndMiniGame();
    }

    protected override string CheckResult()
    {
        return correctAnswers >= targetScore ? "success" : "fail";
    }

    // Публичные методы для настройки игры
    public void SetCustomCards(List<CardData> customCards, bool shuffle = true)
    {
        gameCards = new List<CardData>(customCards);
        if (gameCards.Count > maxCards)
        {
            gameCards = gameCards.GetRange(0, maxCards);
        }
        cardsRemaining = gameCards.Count;
        if (shuffle)
            ShuffleCards();
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

    private void RestoreIconColors()
    {
        var go = currentCardPrefab.gameObject;
        var imgs = go.GetComponentsInChildren<Image>().ToList();

        var successIcon = imgs.Where(ch => ch.name.Contains("success")).FirstOrDefault();
        var declayIcon = imgs.Where(ch => ch.name.Contains("decay")).FirstOrDefault();

        if (successIcon != null)
        {
            if (successIcon.color == defaulColor)
                successIcon.color = successColor;
        }

        if (declayIcon != null)
        {
            if (declayIcon.color == defaulColor)
                declayIcon.color = failureColor;
        }
        Debug.Log("Цвета иконок success/decay восстановлены");
    }

}