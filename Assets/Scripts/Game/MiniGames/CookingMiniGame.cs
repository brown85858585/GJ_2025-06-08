using Game.MiniGames;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        private Image[] winZones = new Image[3];
        private float[] targetAngles = new float[3];
        private bool[] zoneCompleted = new bool[3]; // Отслеживаем выполненные зоны
        private int currentTarget = 0; // Текущая цель (0, 1, 2)
        private int completedZones = 0; // Количество завершенных зон


        [Header("Cooking Settings")]
        public float arcRadius = 150f;
        public float arcStartAngle = 200f;
        public float arcEndAngle = 340f;

        [Header("Game Settings")]
        public float successZoneAngle = 45f;

        [Header("Colors")]
        public Color arcColor = Color.gray;
        public Color successZoneColor = Color.green;
        public Color indicatorColor = Color.black;

        [Header("Prefab Elements References")]
        private Transform knifeHandler;
        private Transform winZoneHandler;
        private Image winZone;
        private RectTransform knife;

        private float currentAngle;
        private float targetAngle;
        private bool movingClockwise = true;






        private void SetupMultipleWinZones()
        {
            for (int i = 0; i < 3; i++)
            {
                if (winZones[i] != null && winZoneHandlers[i] != null)
                {
                    // ЧИТАЕМ реальный угол из Transform, а не задаем фиксированный
                    targetAngles[i] = winZoneHandlers[i].rotation.eulerAngles.z;

                    // Нормализуем угол (-180 до 180)
                    if (targetAngles[i] > 180f)
                        targetAngles[i] -= 360f;

                    zoneCompleted[i] = false;
                    winZones[i].color = successZoneColor;

                    // НЕ поворачиваем - оставляем как в префабе!
                    // winZoneHandlers[i].rotation = Quaternion.Euler(0, 0, targetAngles[i]);

                    Debug.Log($"WinZone {i + 1} реальный угол: {targetAngles[i]}");
                }
            }

            completedZones = 0;
            UpdateInstructionText($"🎯 Попадите в любую из 3 зон");
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
            if (!isGameActive)
            {
                Debug.Log("Игра неактивна, игнорируем нажатие E");
                return;
            }

            int hitZoneIndex = CheckCurrentZone(); // Получаем индекс попавшей зоны

            if (hitZoneIndex >= 0)
            {
                // Попадание в зону с индексом hitZoneIndex
                HideCompletedZone(hitZoneIndex); // Скрываем именно ту зону, в которую попали
                completedZones++;

                if (completedZones >= 3)
                {
                    Debug.Log("🎉 Все зоны выполнены! Победа!");
                    isGameActive = false;
                    UpdateInstructionText("🎉 Отлично! Все зоны выполнены!");
                    OnGameAttempt?.Invoke(true);
                    model.Score += 150;
                    StartCoroutine(ShowResultAndEnd(2f));
                }
                else
                {
                    int remainingZones = 3 - completedZones;
                    UpdateInstructionText($"🎯 Попадите в любую из {remainingZones} оставшихся зон");
                    Debug.Log($"✅ Зона {hitZoneIndex + 1} выполнена! Осталось: {remainingZones}");
                }
            }
            else
            {
                // Промах
                Debug.Log("❌ Промах! Игра окончена!");
                isGameActive = false;
                UpdateInstructionText("❌ Промах! Попробуйте снова!");
                OnGameAttempt?.Invoke(false);
                model.Score += 25;
                StartCoroutine(ShowResultAndEnd(1.5f));
            }
        }

        private void HideCompletedZone(int zoneIndex)
        {
            if (zoneIndex >= 0 && zoneIndex < winZones.Length && winZones[zoneIndex] != null)
            {
                zoneCompleted[zoneIndex] = true;

                // Анимация исчезновения
                StartCoroutine(FadeOutZone(zoneIndex));

                Debug.Log($"Зона {zoneIndex + 1} скрыта");
            }
        }

        private IEnumerator FadeOutZone(int zoneIndex)
        {
            Image zone = winZones[zoneIndex];
            Color startColor = zone.color;
            float duration = 0.5f;
            float elapsedTime = 0f;

            // Плавное исчезновение
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);

                Color newColor = startColor;
                newColor.a = alpha;
                zone.color = newColor;

                yield return null;
            }

            // Полностью скрываем
            zone.gameObject.transform.parent.gameObject.SetActive(false);
        }

        // Сброс для новой игры
        protected override void StartGameLogic()
        {
            // Сбрасываем все зоны
            for (int i = 0; i < 3; i++)
            {
                if (winZones[i] != null)
                {
                    winZones[i].gameObject.SetActive(true);
                    winZones[i].color = successZoneColor;
                    zoneCompleted[i] = false;
                }
            }

            currentTarget = 0;
            completedZones = 0;
            currentAngle = 0f;
            movingClockwise = true;

            UpdateInstructionText("🎯 Попадите в зону 1 из 3");

            if (knife != null)
            {
                UpdateKnifePosition();
            }

            StartCoroutine(MoveKnife());
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
        /*
           public void SetWinZoneWidth(float newWidth)
           {
               Image targetWinZone =  winZone;
               if (targetWinZone != null)
               {
                   RectTransform rect = targetWinZone.rectTransform;
                   rect.sizeDelta = new Vector2(newWidth, rect.sizeDelta.y);
               }
           }

           private void SetupWinZone()
           {
               Image targetWinZone = cookingComponents?.winZone ?? winZone;
               Transform targetWinZoneHandler = cookingComponents?.winZoneHandler ?? winZoneHandler;

               if (targetWinZone != null && targetWinZoneHandler != null)
               {
                   // Устанавливаем случайную позицию для зеленой зоны
                   targetAngle = Random.Range(-60f, 60f); // Диапазон углов для зоны победы

                   // Меняем цвет зоны на зеленый
                   targetWinZone.color = successZoneColor;

                   // Настраиваем размер winZone если включена кастомизация
                   if (useCustomWinZoneSize)
                   {
                       SetWinZoneSize(winZoneWidthSlider, winZoneHeight);
                   }

                   // Поворачиваем зону победы
                   targetWinZoneHandler.rotation = Quaternion.Euler(0, 0, targetAngle);

                   Debug.Log($"Win zone установлена на угол: {targetAngle}, размер: {winZoneWidthSlider}x{winZoneHeight}");
               }
           }
           */

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
            startBg.color = new Color(0, 0, 0, 0.7f);

            //CreateText("Title", "🍳 Готовка еды", new Vector2(0, 100), 24, Color.white, new Vector2(400, 40), startScreen.transform);
            //CreateText("Subtitle", "Алгоритм:", new Vector2(0, 60), 18, Color.yellow, new Vector2(400, 30), startScreen.transform);

            //CreateText("Step1", "1. Игрок нажимает на кухню", new Vector2(0, 20), 14, Color.white, new Vector2(400, 25), startScreen.transform);
            //CreateText("Step2", "2. ГГ подходит к холодильнику, берет пакет, переносит его на стол", new Vector2(0, -5), 14, Color.white, new Vector2(500, 25), startScreen.transform);
            //CreateText("Step3", "3. Запускается анимация нарезки продуктов", new Vector2(0, -30), 14, Color.white, new Vector2(400, 25), startScreen.transform);
            //CreateText("Step4", "4. Мини-игра: остановите нож в зеленой зоне", new Vector2(0, -55), 14, Color.green, new Vector2(400, 25), startScreen.transform);
            //CreateText("Step5", "5. После завершения ГГ садится за стол и ест", new Vector2(0, -80), 14, Color.white, new Vector2(400, 25), startScreen.transform);

            startButton = CreateButton("StartButton", "Начать готовку (Пробел)", new Vector2(-200, 0), new Color(0.2f, 0.8f, 0.2f), new Vector2(300, 100), startScreen.transform, 24);
            startButton.onClick.AddListener(StartGame);

            Button startExitButton = CreateButton("StartExitButton", "Выход", new Vector2(200, 0), Color.gray, new Vector2(300, 100), startScreen.transform, 24);
            startExitButton.onClick.AddListener(ExitMiniGame);
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
            currentSuccessImage.color = successZoneColor;
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
            instructionText = CreateText("InstructionText", "Остановите нож в зеленой зоне!", new Vector2(0, 200), 16, Color.black, new Vector2(300, 40), gameScreen.transform);
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

            while (isGameActive)
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

        protected override void OnDestroy()
        {
            if (instantiatedCookingView != null)
            {
                Destroy(instantiatedCookingView);
            }
            base.OnDestroy();
        }
    }
}