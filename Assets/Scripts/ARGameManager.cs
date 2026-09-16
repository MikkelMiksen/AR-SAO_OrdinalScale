using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Scanning,
    MainMenu,
    Gameplay,
    GameOver
}

public class ARGameManager : MonoBehaviour
{
    public static ARGameManager Instance { get; private set; }

    [Header("Dependencies")]
    public ARSceneManager arSceneManager;
    public EnemySpawnManager enemySpawnManager;
    public UIManager uiManager;

    [Header("Game State")]
    public GameState currentState = GameState.Scanning;

    [Header("Wave Settings")]
    public int currentWave = 0;
    public int enemiesPerWaveBase = 3;
    public float waveInterval = 10f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Only initialize on MainMenu scene
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            InitializeMainMenu();
        }
    }

    private void InitializeMainMenu()
    {
        // Auto-find managers if not assigned
        if (uiManager == null)
            uiManager = FindFirstObjectByType<UIManager>();

        if (arSceneManager == null)
            arSceneManager = FindFirstObjectByType<ARSceneManager>();

        if (arSceneManager != null)
        {
            arSceneManager.OnSceneLoaded -= HandleSceneLoaded;
            arSceneManager.OnSceneLoaded += HandleSceneLoaded;
            SetState(GameState.Scanning);
        }
        else
        {
            Debug.LogWarning("[SAO] ARSceneManager not found. Moving to MainMenu.");
            SetState(GameState.MainMenu);
        }
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (currentState == GameState.MainMenu)
        {
            if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                if (UnityEngine.InputSystem.Keyboard.current.enterKey.wasPressedThisFrame ||
                    UnityEngine.InputSystem.Keyboard.current.numpadEnterKey.wasPressedThisFrame ||
                    UnityEngine.InputSystem.Keyboard.current.lKey.wasPressedThisFrame)
                {
                    Debug.Log("[SAO] Link Start triggered via keyboard.");
                    StartGame();
                }
            }
        }
#endif
    }

    public void SetState(GameState newState)
    {
        currentState = newState;
        Debug.Log($"[SAO] Game State Changed to: {newState}");

        switch (newState)
        {
            case GameState.Scanning:
                if (arSceneManager != null)
                    arSceneManager.StartRoomScan();
                if (uiManager != null)
                    uiManager.ShowScanning();
                break;

            case GameState.MainMenu:
                if (uiManager != null)
                    uiManager.ShowMainMenu();
                break;

            case GameState.Gameplay:
                // Load ARGameplay scene
                SceneManager.LoadScene("ARGameplay", LoadSceneMode.Single);
                break;

            case GameState.GameOver:
                if (uiManager != null)
                {
                    int score = PlayerStats.Instance != null ? PlayerStats.Instance.score : 0;
                    uiManager.ShowGameOver(score, currentWave);
                }
                break;
        }
    }

    private void HandleSceneLoaded()
    {
        if (currentState == GameState.Scanning)
        {
            SetState(GameState.MainMenu);
        }
    }

    public void StartGame()
    {
        if (currentState == GameState.MainMenu)
        {
            SetState(GameState.Gameplay);
        }
    }

    public void OnGameplaySceneLoaded()
    {
        // Called from ARGameplay scene when it loads
        if (enemySpawnManager == null)
            enemySpawnManager = FindFirstObjectByType<EnemySpawnManager>();

        if (uiManager == null)
            uiManager = FindFirstObjectByType<UIManager>();

        // Start the actual gameplay
        currentState = GameState.Gameplay;
        if (uiManager != null)
            uiManager.ShowHUD();
        StartNextWave();
    }

    public void StartNextWave()
    {
        currentWave++;
        int enemyCount = enemiesPerWaveBase + (currentWave * 2);
        Debug.Log($"[SAO] Starting Wave {currentWave} with {enemyCount} enemies.");

        if (enemySpawnManager != null)
        {
            enemySpawnManager.StartWave(enemyCount);
        }
        else
        {
            Debug.LogWarning("[SAO] EnemySpawnManager not found!");
        }
    }

    public void OnWaveCleared()
    {
        Debug.Log($"[SAO] Wave {currentWave} Cleared!");
        Invoke(nameof(StartNextWave), waveInterval);
    }

    public void GameOver()
    {
        SetState(GameState.GameOver);
    }
}
