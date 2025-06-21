using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Unity.VisualScripting;


namespace Game.MiniGames
{

    public abstract class BaseTimingMiniGame : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] protected Canvas mainCanvas;
        [SerializeField] protected GameObject panel;

        public GameObject Panel => panel;
        protected GameObject miniGamePanel { get { return panel; }  set => panel = value; }

        [Header("Input")]
        [SerializeField] protected InputActionReference actionInputAction; // E клавиша
        [SerializeField] protected InputActionReference startInputAction;  // Пробел для старта
        protected bool useDirectInput = false;

        [Header("UI Elements")]
        protected RectTransform indicator;
        protected Button actionButton;
        protected Button exitButton;
        protected Button startButton;
        protected Text instructionText;
        protected GameObject startScreen;
        protected GameObject gameScreen;


        [Header("Game Settings")]
        public float indicatorSpeed = 100f;
        public int maxAttempts = 3;

        public bool isGameActive = false;
        protected bool gameStarted = false;
        protected int currentAttempts = 0;

        // События
        public System.Action OnMiniGameComplete;
        public System.Action<bool> OnGameAttempt;

        protected virtual void Start()
        {
             FindSceneComponents();
             SetupInput();

            HideMiniGameUI();
            CreateMiniGameUI();
            ShowMiniGameUI();
        }


        /// <summary>
        /// ЕДИНСТВЕННОЕ место где показываем UI мини-игры
        /// </summary>
        protected void ShowMiniGameUI()
        {
            Debug.Log("🔵 ПОКАЗЫВАЕМ UI мини-игры");

            if (miniGamePanel != null)
            {
                miniGamePanel.SetActive(true);
                Debug.Log("✅ MiniGamePanel включена");
            }
            else
            {
                Debug.LogError("❌ miniGamePanel == null! UI не может быть показан");
            }
        }

        /// <summary>
        /// ЕДИНСТВЕННОЕ место где скрываем UI мини-игры
        /// </summary>
        protected void HideMiniGameUI()
        {
            Debug.Log("🔴 СКРЫВАЕМ UI мини-игры");

            if (miniGamePanel != null)
            {
                miniGamePanel.SetActive(false);
                Debug.Log("✅ MiniGamePanel выключена");
            }
        }

        public virtual void SetupInput()
        {
            if (actionInputAction == null || startInputAction == null)
            {
                useDirectInput = true;
                Debug.Log("InputActionReference не назначены, используем прямой ввод клавиш");
                return;
            }

            actionInputAction.action.performed += OnActionInput;
            startInputAction.action.performed += OnStartInput;

            Debug.Log("Input System настроена для мини-игры");
        }

        protected virtual void OnDestroy()
        {
            if (actionInputAction != null)
                actionInputAction.action.performed -= OnActionInput;

            if (startInputAction != null)
                startInputAction.action.performed -= OnStartInput;
        }

        protected virtual void Update()
        {
            if (!useDirectInput) return;

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
        }

        protected virtual void OnActionInput(InputAction.CallbackContext context)
        {
            if (gameStarted && isGameActive)
            {
                Debug.Log("E нажата через Input System!");
                OnActionButtonClick();
            }
        }

        protected virtual void OnStartInput(InputAction.CallbackContext context)
        {
            if (!gameStarted)
            {
                Debug.Log("Пробел нажат через Input System - запуск игры!");
                StartGame();
            }
        }

        protected virtual void FindSceneComponents()
        {
            if (mainCanvas == null)
            {
                Debug.LogError("Canvas не найден в сцене!");
                return;
            }

           
            if (miniGamePanel == null)
            { 
                miniGamePanel = GameObject.Find("MiniGamePanel");
                //if (miniGamePanel == null)
                  //  miniGamePanel = Instantiate(panel);
            }

            if (miniGamePanel == null)
            {
                Debug.LogError("MiniGamePanel не найдена в Canvas!");
                return;
            }

            Debug.Log($"Компоненты найдены: Canvas = {mainCanvas.name}, Panel = {miniGamePanel.name}");
        }

        protected virtual void CreateMiniGameUI()
        {
            if (miniGamePanel == null) return;

          
            ClearExistingElements();
            CreateStartScreen();
            CreateGameScreen();

            // Инициализировать массив зон если он не задан


            Debug.Log("Базовая мини-игра готова!");
        }

        protected virtual void ClearExistingElements()
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

        protected abstract void CreateStartScreen();
        protected abstract void CreateGameScreen();
        public abstract void OnActionButtonClick();
        protected abstract string CheckResult();

        // Базовые методы для создания UI элементов
        protected Button CreateButton(string name, string text, Vector2 position, Color color, Vector2 size, Transform parent = null)
        {
            if (parent == null) parent = miniGamePanel.transform;

            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent, false);

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

        public Text CreateText(string name, string text, Vector2 position, int fontSize, Color color, Vector2 size, Transform parent = null)
        {
            if (parent == null) parent = miniGamePanel.transform;

            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);

            Text textComponent = textObj.AddComponent<Text>();
            textComponent.text = text;
            textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComponent.alignment = TextAnchor.MiddleCenter;
            textComponent.color = color;
            textComponent.fontSize = fontSize;

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.sizeDelta = size;
            textRect.anchoredPosition = position;

            return textComponent;
        }

        // Метод для создания игрового объекта с Image компонентом - можно переопределить в наследниках
        protected virtual GameObject CreateImageObject(string name, Image assignedImage, Vector2 size, Vector2 position, Transform parent = null)
        {
            if (parent == null) parent = gameScreen.transform;

            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            Image imageComponent;
            if (assignedImage != null)
            {
                // Использовать предварительно настроенный Image из префаба
                imageComponent = Instantiate(assignedImage, obj.transform);
                imageComponent.transform.localPosition = Vector3.zero;
            }
            else
            {
                // Создать новый Image компонент
                imageComponent = obj.AddComponent<Image>();
                imageComponent.color = Color.white;
            }

            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

            return obj;
        }

        // Публичные методы
        public virtual void StartMiniGame()
        {
            if (miniGamePanel == null)
            {
                Debug.LogError("Мини-игра не инициализирована!");
                return;
            }

            Debug.Log("🎮 Открытие мини-игры (стартовый экран)");

            if (!useDirectInput)
            {
                actionInputAction?.action.Enable();
                startInputAction?.action.Enable();
            }



            if (startScreen != null) startScreen.SetActive(true);
            if (gameScreen != null) gameScreen.SetActive(false);

            gameStarted = false;
            isGameActive = false;

            ShowMiniGameUI();
            Debug.Log("Стартовый экран готов!");
        }

        protected virtual void StartGame()
        {
            Debug.Log("🎮 Запуск игры!");

            if (startScreen != null) startScreen.SetActive(false);
            if (gameScreen != null) gameScreen.SetActive(true);

            gameStarted = true;
            currentAttempts = 0;
            isGameActive = true;

            StartGameLogic();
        }

        protected abstract void StartGameLogic();

        public virtual void EndMiniGame()
        {
            Debug.Log("🎮 Завершение мини-игры");

            isGameActive = false;
            gameStarted = false;

            if (!useDirectInput)
            {
                actionInputAction?.action.Disable();
                startInputAction?.action.Disable();
            }

            if (miniGamePanel != null)
            {
                HideMiniGameUI();
                Debug.Log("MiniGamePanel выключена!");
            }

            OnMiniGameComplete?.Invoke();
        }

        protected virtual void ExitMiniGame()
        {
            Debug.Log("Выход из мини-игры");
            EndMiniGame();
        }

        protected virtual void UpdateInstructionText(string message)
        {
            if (instructionText != null)
            {
                instructionText.text = message;
            }
        }

        protected IEnumerator ShowResultAndEnd(float delay)
        {
            yield return new WaitForSeconds(delay);
            EndMiniGame();
        }
    }
}
