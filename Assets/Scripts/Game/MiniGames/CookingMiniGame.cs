using Game.MiniGames;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace Game.MiniGames
{
    public class CookingMiniGame : BaseTimingMiniGame
    {
        [Header("Prefab Integration")]
        [SerializeField] private CookingMiniGameView currentCookingViewPrefab; // Ссылка на CookingGameView префаб
        [SerializeField] private List<CookingMiniGameView> cookingViewPrefabs; // Префаб для создания UI

        private GameObject instantiatedCookingView;
        private CookingMiniGameView instantiatedCookingGameView;


        [Header("Multiple Win Zones Game")]
        private Transform[] winZoneHandlers = new Transform[3];
        private UIElementTweener[] winZoneHandlersTweens = new UIElementTweener[3];
        private Image[] winZones = new Image[3];
        private float[] targetAngles = new float[3];
        private bool[] zoneCompleted = new bool[3]; // Отслеживаем выполненные зоны
        private int currentTarget = 0; // Текущая цель (0, 1, 2)
        private int completedZones = 0; // Количество завершенных зон

        public int WinCounter { get; set; } = 3;

        [Header("Cooking Settings")]
        public float arcRadius = 150f;
        public float arcStartAngle = 200f;
        public float arcEndAngle = 340f;

        [Header("Game Settings")]
        public float successZoneAngle = 45f;

        [Header("Colors")]
        public Color arcColor = Color.gray;
        //public Color successZoneColor = Color.green;
        public Color indicatorColor = Color.black;

        [Header("Prefab Elements References")]
        private Transform knifeHandler;
        private Transform winZoneHandler;
        private Image winZone;
        private RectTransform knife;

        //private Image actionButtonIndicator;

        private float currentAngle;
        private float targetAngle;
        private bool movingClockwise = true;

        [Header("Attempts System")]
        private int maxGameAttempts = 3; // Максимальное количество нажатий E
        private int usedAttempts = 0; // Количество использованных нажатий E

        private bool isPaused = true;

        private Coroutine moveKnifeRoutine = null;

        private void SetupMultipleWinZones()
        {
            for (int i = 0; i < 3; i++)
            {
                if (winZones[i] != null && winZoneHandlers[i] != null)
                {
                    targetAngles[i] = winZoneHandlers[i].rotation.eulerAngles.z;

                    if (targetAngles[i] > 180f)
                        targetAngles[i] -= 360f;

                    zoneCompleted[i] = false;
                    //winZones[i].color = successZoneColor;

                    Debug.Log($"WinZone {i + 1} реальный угол: {targetAngles[i]}");
                }
            }

            completedZones = 0;
            usedAttempts = 0; // ДОБАВИТЬ ЭТУ СТРОКУ
            //UpdateInstructionText($"🎯 Попадите в любую из 3 зон (Попытки: {maxGameAttempts})");
            UpdateInstructionText($"{maxGameAttempts - usedAttempts}");
        }

        private int CheckCurrentZone()
        {
            float tolerance = 15f;

            // Проверяем попадание в любую из НЕ завершенных зон
            for (int i = 0; i < targetAngles.Length; i++)
            {
                if (!zoneCompleted[i]) // Зона еще не выполнена
                {
                    float targetAngle = targetAngles[i];
                    if (Mathf.Abs(currentAngle - targetAngle) <= tolerance)
                    {
                        Debug.Log($"✅ Попадание в зону {i + 1}! Угол: {targetAngle}");
                        return i; // Возвращаем индекс найденной зоны
                    }
                }
            }

            Debug.Log($"❌ Промах! Текущий угол: {currentAngle}");
            return -1; // Промах
        }

        protected override void OnActionButtonClick()
        {
            if (!isGameActive || isPaused)
            {
                Debug.Log("Игра неактивна, игнорируем нажатие E");
                return;
            }

            // Анимируем кнопку
            instantiatedCookingGameView.AnimateActionButton();

            // Увеличиваем счетчик использованных попыток при каждом нажатии E
            usedAttempts++;
            int remainingAttempts = maxGameAttempts - usedAttempts;

            Debug.Log($"Нажатие E #{usedAttempts} из {maxGameAttempts}");

            int hitZoneIndex = CheckCurrentZone(); // Проверяем попадание

            if (hitZoneIndex >= 0)
            {
                // Попадание в зону
                HideCompletedZone(hitZoneIndex);
                completedZones++;
                Debug.Log($"✅ Зона {hitZoneIndex + 1} выполнена! Попаданий: {completedZones}/3");

                if (completedZones >= 3)
                {
                    // Все зоны выполнены - победа!
                    Debug.Log("🎉 Все зоны выполнены! Победа!");
                    isGameActive = false;
                    //UpdateInstructionText("🎉 Отлично! Все зоны выполнены!");
                    OnGameAttempt?.Invoke(true);
                    model.Score += 150;
                    StartCoroutine(ShowResultAndEnd(2f));
                    return;
                }
            }
            else
            {
                // Промах
                WinCounter--;                
                Debug.Log($"❌ Промах! Попытка {usedAttempts}");
            }

            // Проверяем остались ли попытки
            if (usedAttempts >= maxGameAttempts)
            {
                // Все попытки исчерпаны
                bool isVictory = completedZones >= maxAttempts;
                Debug.Log($"Все {maxGameAttempts} попытки использованы. Выполнено зон: {completedZones}/3");

                isGameActive = false;

                if (isVictory)
                {
                    //UpdateInstructionText("🎉 Победа! Все зоны выполнены!");
                    OnGameAttempt?.Invoke(true);
                }
                else
                {
                    //UpdateInstructionText($"⏰ Попытки закончились! Выполнено: {completedZones}/3 зон");
                    OnGameAttempt?.Invoke(false);
                    //model.Score += completedZones * 25;
                }

                StartCoroutine(ShowResultAndEnd(2f));
            }
            else
            {
                // Есть еще попытки - обновляем инструкции
                int remainingZones = maxGameAttempts - completedZones;
                //UpdateInstructionText($"🎯 Попадите в {remainingZones} зон (Попыток: {remainingAttempts})");
                UpdateInstructionText($"{maxGameAttempts - usedAttempts}");
            }
        }

        private void HideCompletedZone(int zoneIndex)
        {
            if (zoneIndex >= 0 && zoneIndex < winZones.Length && winZones[zoneIndex] != null)
            {
                zoneCompleted[zoneIndex] = true;
                model.Score += 50;
                // Анимация исчезновения
                //StartCoroutine(FadeOutZone(zoneIndex));
                winZoneHandlersTweens[zoneIndex].Hide();

                Debug.Log($"Зона {zoneIndex + 1} скрыта");
            }
        }

        //private IEnumerator FadeOutZone(int zoneIndex)
        //{
        //    Image zone = winZones[zoneIndex];
        //    Color startColor = zone.color;
        //    float duration = 0.5f;
        //    float elapsedTime = 0f;

        //    // Плавное исчезновение
        //    while (elapsedTime < duration)
        //    {
        //        elapsedTime += Time.deltaTime;
        //        float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);

        //        Color newColor = startColor;
        //        newColor.a = alpha;
        //        zone.color = newColor;

        //        yield return null;
        //    }

        //    // Полностью скрываем
        //    zone.gameObject.transform.parent.gameObject.SetActive(false);
        //}

        // Сброс для новой игры
        protected override void StartGameLogic()
        {
            // Сбрасываем все зоны
            for (int i = 0; i < 3; i++)
            {
                if (winZones[i] != null)
                {
                    winZones[i].gameObject.SetActive(true);
                    //winZones[i].color = successZoneColor;
                    zoneCompleted[i] = false;
                }
            }

            currentTarget = 0;
            completedZones = 0;
            usedAttempts = 0; // ДОБАВИТЬ ЭТУ СТРОКУ - сбрасываем счетчик нажатий
            currentAngle = 90f;
            movingClockwise = true;

            //UpdateInstructionText($"🎯 Попадите в 3 зоны за {maxGameAttempts} нажатий E");
            UpdateInstructionText($"{maxGameAttempts - usedAttempts}");

            if (knife != null)
            {
                UpdateKnifePosition();
            }

            if (moveKnifeRoutine != null)
            {
                StopCoroutine(moveKnifeRoutine);
            }

            moveKnifeRoutine = StartCoroutine(MoveKnife());
        }

        private void SetupPrefabReferences()
        {
            knifeHandler = instantiatedCookingView.transform.Find("Panel/knifeHandler");

            // Находим все 3 winZone по именам из иерархии
            string[] zoneNames = { "winZone", "winZone", "winZone" };

            for (int i = 0; i < 3; i++)
            {
                // Ищем по полному пути
                Transform foundZone = instantiatedCookingView.transform.Find($"Panel/winZoneHandler{i}/{zoneNames[i]}");
                if (foundZone != null)
                {
                    winZoneHandlers[i] = foundZone;
                    winZoneHandlersTweens[i] = foundZone.parent.gameObject.GetComponent<UIElementTweener>();
                    winZones[i] = foundZone.GetComponent<Image>();
                    Debug.Log($"WinZone {i + 1} найдена: {zoneNames[i]}");
                }
                else
                {
                    Debug.LogError($"WinZone {i + 1} не найдена: {zoneNames[i]}");
                }
            }

            if (knifeHandler != null)
            {
                knife = knifeHandler.GetComponent<RectTransform>();
            }

            SetupMultipleWinZones();
        }

        // Обновленный метод для конкретной зоны
        public void SetWinZoneWidth(int zoneIndex, float newWidth)
        {
            if (zoneIndex >= 0 && zoneIndex < winZones.Length && winZones[zoneIndex] != null)
            {
                RectTransform rect = winZones[zoneIndex].rectTransform;
                rect.sizeDelta = new Vector2(newWidth, rect.sizeDelta.y);
                Debug.Log($"WinZone {zoneIndex + 1} размер установлен: {newWidth}");
            }
        }

        // Обновленная проверка попадания в любую из 3 зон
        protected override string CheckResult()
        {
            float tolerance = 15f; // Допустимое отклонение

            for (int i = 0; i < targetAngles.Length; i++)
            {
                if (winZones[i] != null && Mathf.Abs(currentAngle - targetAngles[i]) <= tolerance)
                {
                    Debug.Log($"✅ Попадание в WinZone {i + 1}! Угол: {targetAngles[i]}");
                    return "success";
                }
            }

            Debug.Log($"❌ Промах! Текущий угол: {currentAngle}");
            return "fail";
        }
/*
        protected override void Start()
        {
            instantiatedCookingView= cookingViewPrefabs[MiniGameCoordinator.DayLevel].CookingViewPrefab;
            indicatorSpeed = cookingViewPrefabs[MiniGameCoordinator.DayLevel].gameSpeed;
        

            // Если префаб не назначен, попробуем найти его в Resources
            // if (cookingViewPrefab == null)
            // {
            //     cookingViewPrefab = Resources.Load<GameObject>("Prefabs/MiniGame/CookingGameView");
            // }

            base.Start();
        }
*/
        protected override void CreateStartScreen()
        {
            // Создаем пустой startScreen объект чтобы не было null
            startScreen = new GameObject("StartScreen");
            startScreen.transform.SetParent(miniGamePanel.transform, false);
            startScreen.SetActive(false); // Сразу скрываем

            Debug.Log("Стартовый экран создан но скрыт");
        }

        protected override void CreateGameScreen()
        {
            // Создаем gameScreen как обычно
            gameScreen = new GameObject("GameScreen");
            gameScreen.transform.SetParent(miniGamePanel.transform, false);
            gameScreen.SetActive(false);

            RectTransform gameRect = gameScreen.AddComponent<RectTransform>();
            gameRect.anchorMin = Vector2.zero;
            gameRect.anchorMax = Vector2.one;
            gameRect.offsetMin = Vector2.zero;
            gameRect.offsetMax = Vector2.zero;

            // Создаем префаб как отдельный Canvas поверх всего
            InstantiateCookingView();

            // Создаем дополнительные UI элементы управления в gameScreen
            CreateGameButtons();
            CreateInstructionText();
        }

        private void InstantiateCookingView()
        {
            if (cookingViewPrefabs[MiniGameCoordinator.DayLevel].CookingViewPrefab != null)
            {
                //var origin = cookingViewPrefabs[MiniGameCoordinator.DayLevel].CookingViewPrefab;
                // Создаем префаб в gameScreen, но сохраняем его Canvas
                instantiatedCookingGameView = Instantiate(cookingViewPrefabs[MiniGameCoordinator.DayLevel], gameScreen.transform);
                instantiatedCookingView = instantiatedCookingGameView.gameObject;

                instantiatedCookingGameView.OnAnimationComplete += InstantiatedCookingGameView_OnAnimationComplete;

                // Настраиваем RectTransform для полного заполнения
                
                RectTransform viewRect = instantiatedCookingView.GetComponent<RectTransform>();
                if (viewRect != null)
                {
                    viewRect.anchorMin = Vector2.zero;
                    viewRect.anchorMax = Vector2.one;
                    viewRect.offsetMin = Vector2.zero;
                    viewRect.offsetMax = Vector2.zero;
                    viewRect.localScale = Vector3.one;
                }

                // Настраиваем Canvas префаба
                Canvas prefabCanvas = instantiatedCookingView.GetComponent<Canvas>();
                if (prefabCanvas != null)
                {
                    // Оставляем Canvas включенным, но делаем его дочерним
                    prefabCanvas.overrideSorting = true;
                    prefabCanvas.sortingOrder = 1; // Поверх других элементов в gameScreen
                    Debug.Log("Canvas префаба настроен как дочерний");
                }

                SetupPrefabReferences();
            }
            else
            {
                Debug.LogError("CookingGameView префаб не найден! Убедитесь что он находится в Resources/Prefabs/MiniGame/");
                // Fallback к старому методу создания UI
                CreateFallbackUI();
            }
        }

        private void InstantiatedCookingGameView_OnAnimationComplete()
        {
            isPaused = false;

            if (moveKnifeRoutine != null)
            {
                StopCoroutine(moveKnifeRoutine);
            }

            moveKnifeRoutine = StartCoroutine(MoveKnife());
        }

        /*
        private void SetupPrefabReferences()
        {
            

            // Находим элементы в префабе
            knifeHandler = instantiatedCookingView.transform.Find("Panel/knifeHandler");
            winZoneHandler = instantiatedCookingView.transform.Find("Panel/winZoneHandler");
           
            if (knifeHandler != null)
            {
                knife = knifeHandler.GetComponent<RectTransform>();
                Debug.Log("Knife handler найден!");
            }
            else
            {
                Debug.LogError("knifeHandler не найден в префабе!");
            }

            if (winZoneHandler != null)
            {
                winZone = winZoneHandler.GetComponentInChildren<Image>();
               
                SetupWinZone();
                Debug.Log("Win zone handler найден!");
            }
            else
            {
                Debug.LogError("winZoneHandler не найден в префабе!");
            }
        }
        */

        protected override void FindSceneComponents()
        {
            if (mainCanvas == null)
            {
                Debug.LogError("Canvas не найден в сцене!");
                return;
            }

            miniGamePanel = GameObject.Find("CookingMiniGamePanel");
            if (miniGamePanel == null)
            {
                Debug.LogError("MiniGamePanel не найдена в Canvas!");
                return;
            }

            Debug.Log($"Компоненты найдены: Canvas = {mainCanvas.name}, Panel = {miniGamePanel.name}");
        }

        /*
        private void SetupWinZone()
        {
            if (winZone != null)
            {
                // Устанавливаем случайную позицию для зеленой зоны
                targetAngle = Random.Range(-60f, 60f); // Диапазон углов для зоны победы

                // Меняем цвет зоны на зеленый
                winZone.color = successZoneColor;

                // Поворачиваем зону победы
                winZoneHandler.rotation = Quaternion.Euler(0, 0, targetAngle);

                SetWinZoneWidth(cookingViewPrefabs[MiniGameCoordinator.DayLevel].winZoneWidth);

                Debug.Log($"Win zone установлена на угол: {targetAngle}");
            }
        }
        */

        private void CreateFallbackUI()
        {
            // Старый метод создания UI на случай если префаб не загрузился
            CreateArc();
            CreateSuccessZone();
            CreateIndicator();
        }

        private void CreateArc()
        {
            GameObject arcObj = CreateImageObject("Arc", arcImage, new Vector2(arcRadius * 2, arcRadius * 2), Vector2.zero);
            Image currentArcImage = arcObj.GetComponentInChildren<Image>() ?? arcObj.GetComponent<Image>();
            currentArcImage.color = arcColor;
            currentArcImage.sprite = CreateCircleSprite();
            currentArcImage.type = Image.Type.Filled;
            currentArcImage.fillMethod = Image.FillMethod.Radial360;
            currentArcImage.fillOrigin = 2;
            currentArcImage.fillAmount = (arcEndAngle - arcStartAngle) / 360f;

            RectTransform arcBackground = arcObj.GetComponent<RectTransform>();
            arcBackground.rotation = Quaternion.Euler(0, 0, -arcStartAngle);
        }

        private void CreateSuccessZone()
        {
            GameObject successObj = CreateImageObject("SuccessZone", successZoneImage, new Vector2(arcRadius * 2, arcRadius * 2), Vector2.zero);
            Image currentSuccessImage = successObj.GetComponentInChildren<Image>() ?? successObj.GetComponent<Image>();
            //currentSuccessImage.color = successZoneColor;
            currentSuccessImage.sprite = CreateCircleSprite();
            currentSuccessImage.type = Image.Type.Filled;
            currentSuccessImage.fillMethod = Image.FillMethod.Radial360;
            currentSuccessImage.fillOrigin = 2;
            currentSuccessImage.fillAmount = successZoneAngle / 360f;

            RectTransform successZone = successObj.GetComponent<RectTransform>();
            targetAngle = Random.Range(arcStartAngle + successZoneAngle / 2, arcEndAngle - successZoneAngle / 2);
            successZone.rotation = Quaternion.Euler(0, 0, -targetAngle);
        }

        private void CreateIndicator()
        {
            GameObject indicatorObj = CreateImageObject("Indicator", indicatorImage, new Vector2(10, arcRadius + 20), Vector2.zero);
            Image currentIndicatorImage = indicatorObj.GetComponentInChildren<Image>() ?? indicatorObj.GetComponent<Image>();
            currentIndicatorImage.color = indicatorColor;

            indicator = indicatorObj.GetComponent<RectTransform>();
            indicator.pivot = new Vector2(0.5f, 0f);

            currentAngle = arcStartAngle;
            UpdateIndicatorPosition();
        }

        private void CreateGameButtons()
        {
            // Создаем кнопки в gameScreen
            actionButton = CreateButton("ActionButton", "Остановить (E)", new Vector2(-100, -200), new Color(0.2f, 0.6f, 1f), new Vector2(120, 40), gameScreen.transform);
            actionButton.onClick.AddListener(OnActionButtonClick);
            actionButton.gameObject.SetActive(false);

            exitButton = CreateButton("ExitButton", "Выход", new Vector2(100, -200), Color.gray, new Vector2(80, 40), gameScreen.transform);
            exitButton.onClick.AddListener(ExitMiniGame);
            exitButton.gameObject.SetActive(false);
        }

        private void CreateInstructionText()
        {
            // Создаем текст инструкций в gameScreen
            instructionText = CreateText("InstructionText", "3", new Vector2(0, 250), 64, Color.black, new Vector2(300, 80), gameScreen.transform);
        }

        private Sprite CreateCircleSprite()
        {
            Texture2D texture = new Texture2D(100, 100);
            Color[] pixels = new Color[100 * 100];
            Vector2 center = new Vector2(50, 50);
            float radius = 45f;

            for (int y = 0; y < 100; y++)
            {
                for (int x = 0; x < 100; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    if (distance <= radius && distance >= radius - 10)
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



        private IEnumerator MoveKnife()
        {
            float angleRange = 180f; // Диапазон движения ножа (-60 до +60 градусов)

            while (isGameActive && !isPaused)
            {
                float angleSpeed = indicatorSpeed * Time.deltaTime;

                if (movingClockwise)
                {
                    currentAngle += angleSpeed;
                    if (currentAngle >= angleRange / 2)
                    {
                        currentAngle = angleRange / 2;
                        movingClockwise = false;
                    }
                }
                else
                {
                    currentAngle -= angleSpeed;
                    if (currentAngle <= -angleRange / 2)
                    {
                        currentAngle = -angleRange / 2;
                        movingClockwise = true;
                    }
                }

                if (knife != null)
                {
                    UpdateKnifePosition();
                }
                else if (indicator != null)
                {
                    UpdateIndicatorPosition();
                }

                yield return null;
            }
        }

        private void UpdateKnifePosition()
        {
            if (knife != null)
            {
                knife.rotation = Quaternion.Euler(0, 0, currentAngle);
            }
        }

        private void UpdateIndicatorPosition()
        {
            if (indicator != null)
            {
                indicator.rotation = Quaternion.Euler(0, 0, -currentAngle);
            }
        }




        /*
        protected override string CheckResult()
        {
            // Проверяем попадание в зону победы
            float tolerance = 20f; // Допустимое отклонение в градусах

            if (Mathf.Abs(currentAngle - targetAngle) <= tolerance)
            {
                return "success";
            }

            return "fail";
        }
        */

        public void SetDifficulty(float speed, float zoneSize)
        {
            //indicatorSpeed = speed;
            successZoneAngle = zoneSize;
        }

        public void StartCooking()
        {
            StartMiniGame();
        }


        public override void StartMiniGame()
        {
            // Проверяем что скорость установлена
            if (cookingViewPrefabs != null && cookingViewPrefabs.Count > MiniGameCoordinator.DayLevel)
            {
                indicatorSpeed = cookingViewPrefabs[MiniGameCoordinator.DayLevel].gameSpeed;
                Debug.Log($"Скорость повторно проверена: {indicatorSpeed}");
            }

            SetupInput();
            if (miniGamePanel == null)
            {
                Debug.LogError("Мини-игра не инициализирована!");
                return;
            }

            Debug.Log("🎮 Запуск игры сразу без стартового экрана");

            if (!useDirectInput)
            {
                actionInputAction?.action.Enable();
                startInputAction?.action.Enable();
            }

            miniGamePanel.SetActive(true);

            if (startScreen != null) startScreen.SetActive(false);
            if (gameScreen != null) gameScreen.SetActive(true);

            gameStarted = true;
            isGameActive = true;
            currentAttempts = 0;

            StartGameLogic();
        }

        protected override void Start()
        {
            // Сначала инициализируем данные
            if (cookingViewPrefabs != null && cookingViewPrefabs.Count > MiniGameCoordinator.DayLevel)
            {
                instantiatedCookingView = cookingViewPrefabs[MiniGameCoordinator.DayLevel].CookingViewPrefab;
                indicatorSpeed = cookingViewPrefabs[MiniGameCoordinator.DayLevel].gameSpeed;
                Debug.Log($"Скорость установлена: {indicatorSpeed}");
            }
            else
            {
                Debug.LogError("cookingViewPrefabs не настроены!");
                indicatorSpeed = 100f; // Значение по умолчанию
            }

            base.Start(); // Вызываем базовый Start ПОСЛЕ установки скорости
        }

        protected override void OnDestroy()
        {
            if (instantiatedCookingGameView != null)
            {
                instantiatedCookingGameView.OnAnimationComplete -= InstantiatedCookingGameView_OnAnimationComplete;
            }

            if (instantiatedCookingView != null)
            {
                Destroy(instantiatedCookingView);
            }
            base.OnDestroy();
        }
    }
}