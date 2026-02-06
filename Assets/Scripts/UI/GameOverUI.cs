using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Spelunky {

    public class GameOverUI : MonoBehaviour {

        private const int DefaultSortingOrder = 100;
        private const float DefaultScaleFactor = 2f;
        private static readonly Color DefaultOverlayColor = new Color(0f, 0f, 0f, 0.75f);
        private static readonly Color DefaultButtonColor = new Color(0.15f, 0.15f, 0.15f, 0.95f);
        private static readonly Color DefaultButtonHighlightColor = new Color(0.25f, 0.25f, 0.25f, 0.95f);
        private static readonly Color DefaultButtonPressedColor = new Color(0.1f, 0.1f, 0.1f, 0.95f);

        public static GameOverUI Instance { get; private set; }

        [Header("Style")]
        [SerializeField] private Font defaultFont;
        [SerializeField] private Color overlayColor = DefaultOverlayColor;
        [SerializeField] private Color buttonColor = DefaultButtonColor;
        [SerializeField] private Color buttonHighlightColor = DefaultButtonHighlightColor;
        [SerializeField] private Color buttonPressedColor = DefaultButtonPressedColor;

        [Header("Text")]
        [SerializeField] private string titleText = "GAME OVER";
        [SerializeField] private string scoreLabelText = "SCORE";
        [SerializeField] private string restartLabelText = "RESTART";

        private GameObject _panel;
        private Text _scoreValueText;
        private Button _restartButton;
        private bool _isBuilt;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap() {
            if (FindObjectOfType<GameManager>() == null) {
                return;
            }

            EnsureInstance();
        }

        public static void ShowGameOver(int score) {
            Debug.Log($"Showing Game Over UI with score: {score}");
            GameOverUI ui = EnsureInstance();
            ui.Show(score);
        }

        private static GameOverUI EnsureInstance() {
            if (Instance != null) {
                return Instance;
            }

            GameOverUI existing = FindObjectOfType<GameOverUI>();
            if (existing != null) {
                Instance = existing;
                return existing;
            }

            GameObject root = new GameObject("GameOverUI");
            return root.AddComponent<GameOverUI>();
        }

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            BuildUI();
            Hide();
        }

        private void OnDestroy() {
            if (Instance == this) {
                Instance = null;
            }
        }

        private void BuildUI() {
            if (_isBuilt) {
                return;
            }

            Canvas canvas = gameObject.GetComponent<Canvas>();
            if (canvas == null) {
                canvas = gameObject.AddComponent<Canvas>();
            }
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = DefaultSortingOrder;
            canvas.pixelPerfect = true;

            CanvasScaler scaler = gameObject.GetComponent<CanvasScaler>();
            if (scaler == null) {
                scaler = gameObject.AddComponent<CanvasScaler>();
            }
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor = DefaultScaleFactor;
            scaler.referencePixelsPerUnit = 1f;

            if (gameObject.GetComponent<GraphicRaycaster>() == null) {
                gameObject.AddComponent<GraphicRaycaster>();
            }

            RectTransform rootRect = gameObject.GetComponent<RectTransform>();
            if (rootRect == null) {
                rootRect = gameObject.AddComponent<RectTransform>();
            }
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.anchoredPosition = Vector2.zero;
            rootRect.sizeDelta = Vector2.zero;

            _panel = new GameObject("GameOverPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            _panel.transform.SetParent(transform, false);

            RectTransform panelRect = _panel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = Vector2.zero;

            Image panelImage = _panel.GetComponent<Image>();
            panelImage.color = overlayColor;

            Font fontToUse = defaultFont != null ? defaultFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            Text title = CreateText("Title", _panel.transform, fontToUse, titleText, 20, FontStyle.Bold);
            RectTransform titleRect = title.GetComponent<RectTransform>();
            titleRect.sizeDelta = new Vector2(280f, 32f);
            titleRect.anchoredPosition = new Vector2(0f, 52f);

            Text scoreLabel = CreateText("ScoreLabel", _panel.transform, fontToUse, scoreLabelText, 12, FontStyle.Normal);
            RectTransform scoreLabelRect = scoreLabel.GetComponent<RectTransform>();
            scoreLabelRect.sizeDelta = new Vector2(200f, 20f);
            scoreLabelRect.anchoredPosition = new Vector2(0f, 15f);

            _scoreValueText = CreateText("ScoreValue", _panel.transform, fontToUse, "0", 16, FontStyle.Bold);
            RectTransform scoreValueRect = _scoreValueText.GetComponent<RectTransform>();
            scoreValueRect.sizeDelta = new Vector2(200f, 24f);
            scoreValueRect.anchoredPosition = new Vector2(0f, -5f);

            _restartButton = CreateButton("RestartButton", _panel.transform, fontToUse, restartLabelText);
            RectTransform buttonRect = _restartButton.GetComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(140f, 26f);
            buttonRect.anchoredPosition = new Vector2(0f, -55f);

            _restartButton.onClick.AddListener(Restart);

            _isBuilt = true;
        }

        private Text CreateText(string name, Transform parent, Font font, string text, int fontSize, FontStyle style) {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);

            Text textComponent = go.GetComponent<Text>();
            textComponent.font = font;
            textComponent.text = text;
            textComponent.fontSize = fontSize;
            textComponent.fontStyle = style;
            textComponent.alignment = TextAnchor.MiddleCenter;
            textComponent.color = Color.white;
            textComponent.raycastTarget = false;

            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;

            return textComponent;
        }

        private Button CreateButton(string name, Transform parent, Font font, string label) {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);

            Image image = go.GetComponent<Image>();
            image.color = buttonColor;

            Button button = go.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = buttonColor;
            colors.highlightedColor = buttonHighlightColor;
            colors.pressedColor = buttonPressedColor;
            colors.selectedColor = buttonHighlightColor;
            button.colors = colors;

            Text labelText = CreateText("Label", go.transform, font, label, 14, FontStyle.Bold);
            labelText.raycastTarget = false;

            RectTransform labelRect = labelText.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.anchoredPosition = Vector2.zero;
            labelRect.sizeDelta = Vector2.zero;

            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;

            return button;
        }

        private void Show(int score) {
            if (!_isBuilt) {
                BuildUI();
            }

            if (_scoreValueText != null) {
                _scoreValueText.text = score.ToString();
            }

            if (_panel != null) {
                _panel.SetActive(true);
            }

            EnsureEventSystem();

            if (_restartButton != null) {
                _restartButton.Select();
            }
        }

        private void Hide() {
            if (_panel != null) {
                _panel.SetActive(false);
            }
        }

        private void Restart() {
            Scene activeScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(activeScene.buildIndex);
        }

        private static void EnsureEventSystem() {
            if (EventSystem.current != null) {
                return;
            }

            GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            eventSystem.transform.SetParent(null);
        }
    }

}
