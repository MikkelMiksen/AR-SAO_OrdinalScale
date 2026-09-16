using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Documents")]
    public UIDocument uiDocument;
    
    [Header("Visual Tree Assets")]
    public VisualTreeAsset scanningTemplate;
    public VisualTreeAsset mainMenuTemplate;
    public VisualTreeAsset hudTemplate;
    public VisualTreeAsset gameOverTemplate;

    private VisualElement _root;
    private VisualElement _currentScreen;

    // HUD Elements
    private VisualElement _healthBarFill;
    private VisualElement _staminaBarFill;
    private Label _waveText;
    private Label _enemyCountText;
    private Label _scoreText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) { Destroy(gameObject); return; }

        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
        if (uiDocument != null) _root = uiDocument.rootVisualElement;
    }

    private void Start()
    {
        if (_root == null && uiDocument != null)
        {
            _root = uiDocument.rootVisualElement;
        }
    }

    private void SetScreen(VisualTreeAsset template)
    {
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
        if (uiDocument != null) _root = uiDocument.rootVisualElement;

        if (_root == null || template == null) return;
        
        _root.Clear();
        _currentScreen = template.CloneTree();
        _currentScreen.style.flexGrow = 1;
        _currentScreen.style.width = Length.Percent(100f);
        _currentScreen.style.height = Length.Percent(100f);
        _root.Add(_currentScreen);

        PositionCanvas(transform);
    }

    public void ShowScanning()
    {
        SetScreen(scanningTemplate);
    }

    public void ShowMainMenu()
    {
        SetScreen(mainMenuTemplate);
        var playButton = _root.Q<Button>("PlayButton");
        if (playButton != null)
        {
            playButton.clicked += () => {
                if (ARGameManager.Instance != null) ARGameManager.Instance.StartGame();
            };
        }
    }

    public void ShowHUD()
    {
        SetScreen(hudTemplate);
        _healthBarFill = _root.Q<VisualElement>("HealthBarFill");
        _staminaBarFill = _root.Q<VisualElement>("StaminaBarFill");
        _waveText = _root.Q<Label>("WaveText");
        _enemyCountText = _root.Q<Label>("EnemyCountText");
        _scoreText = _root.Q<Label>("ScoreText");
    }

    public void ShowGameOver(int score, int wave)
    {
        SetScreen(gameOverTemplate);
        var scoreLabel = _root.Q<Label>("FinalScoreText");
        var waveLabel = _root.Q<Label>("FinalWaveText");
        var retryButton = _root.Q<Button>("RetryButton");

        if (scoreLabel != null) scoreLabel.text = $"Score: {score}";
        if (waveLabel != null) waveLabel.text = $"Wave: {wave}";
        if (retryButton != null)
        {
            retryButton.clicked += () => {
                UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            };
        }
    }

    private void Update()
    {
        PositionCanvas(transform);

        #if UNITY_EDITOR
        if (ARGameManager.Instance != null && ARGameManager.Instance.currentState == GameState.GameOver)
        {
            if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.rKey.wasPressedThisFrame)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            }
        }
        #endif
    }

    private void PositionCanvas(Transform canvasTransform, float distance = 1.8f)
    {
        Transform camTransform = Camera.main != null ? Camera.main.transform : null;
        if (camTransform == null && ARGameManager.Instance != null)
        {
            var cam = FindFirstObjectByType<Camera>();
            if (cam != null) camTransform = cam.transform;
        }

        if (camTransform != null)
        {
            Vector3 targetPos = camTransform.position + camTransform.forward * distance;
            canvasTransform.position = Vector3.Lerp(canvasTransform.position, targetPos, Time.deltaTime * 5f);
            canvasTransform.LookAt(camTransform.position);
            canvasTransform.Rotate(0, 180, 0);
        }
    }

    public void UpdateHUD(float healthPercent, float staminaPercent, int wave, int enemiesRemaining, int score)
    {
        float hpClamp = Mathf.Clamp01(float.IsNaN(healthPercent) ? 0f : healthPercent);
        float spClamp = Mathf.Clamp01(float.IsNaN(staminaPercent) ? 0f : staminaPercent);

        if (_healthBarFill != null) _healthBarFill.style.width = Length.Percent(hpClamp * 100f);
        if (_staminaBarFill != null) _staminaBarFill.style.width = Length.Percent(spClamp * 100f);
        if (_waveText != null) _waveText.text = $"Wave: {wave}";
        if (_enemyCountText != null) _enemyCountText.text = $"Enemies: {enemiesRemaining}";
        if (_scoreText != null) _scoreText.text = $"Score: {score}";
    }
}
