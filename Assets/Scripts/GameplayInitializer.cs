using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Initializes the ARGameplay scene and ensures all managers are ready.
/// This script runs when ARGameplay loads.
/// </summary>
public class GameplayInitializer : MonoBehaviour
{
    private void Awake()
    {
        // Ensure we have ARSceneManager in this scene
        if (ARSceneManager.Instance == null)
        {
            Debug.LogError("[SAO] ARSceneManager not found in ARGameplay scene!");
        }

        // Ensure we have EnemySpawnManager in this scene
        if (EnemySpawnManager.Instance == null)
        {
            Debug.LogError("[SAO] EnemySpawnManager not found in ARGameplay scene!");
        }
    }

    private void Start()
    {
        // Notify ARGameManager that gameplay scene is loaded
        if (ARGameManager.Instance != null)
        {
            ARGameManager.Instance.OnGameplaySceneLoaded();
        }
        else
        {
            Debug.LogError("[SAO] ARGameManager not found! Game flow broken.");
        }
    }
}
