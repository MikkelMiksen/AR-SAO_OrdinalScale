using UnityEngine;

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

    [Header("Game State")]
    public GameState currentState = GameState.Scanning;

    [Header("Wave Settings")]
    public int currentWave = 0;
    public int enemiesPerWaveBase = 3;
    public float waveInterval = 10f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (arSceneManager == null)
            arSceneManager = FindFirstObjectByType<ARSceneManager>();

        if (enemySpawnManager == null)
            enemySpawnManager = FindFirstObjectByType<EnemySpawnManager>();
    }

    private void Start()
    {
        if (arSceneManager != null)
        {
            arSceneManager.OnSceneLoaded -= HandleSceneLoaded;
            arSceneManager.OnSceneLoaded += HandleSceneLoaded;
        }

        SetState(GameState.Scanning);
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
                if (arSceneManager != null) arSceneManager.StartRoomScan();
                if (UIManager.Instance != null) UIManager.Instance.ShowScanning();
                break;
            case GameState.MainMenu:
                if (UIManager.Instance != null) UIManager.Instance.ShowMainMenu();
                break;
            case GameState.Gameplay:
                if (UIManager.Instance != null) UIManager.Instance.ShowHUD();
                StartNextWave();
                break;
            case GameState.GameOver:
                if (UIManager.Instance != null) 
                {
                    int score = PlayerStats.Instance != null ? PlayerStats.Instance.score : 0;
                    UIManager.Instance.ShowGameOver(score, currentWave);
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

    public void StartNextWave()
    {
        currentWave++;
        int enemyCount = enemiesPerWaveBase + (currentWave * 2);
        Debug.Log($"[SAO] Starting Wave {currentWave} with {enemyCount} enemies.");
        
        if (enemySpawnManager != null)
        {
            enemySpawnManager.StartWave(enemyCount);
        }
    }

    public void OnWaveCleared()
    {
        Debug.Log($"[SAO] Wave {currentWave} Cleared!");
        Invoke(nameof(StartNextWave), waveInterval);
    }
}
