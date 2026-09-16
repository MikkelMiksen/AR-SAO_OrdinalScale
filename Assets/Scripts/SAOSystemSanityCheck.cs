using UnityEngine;

/// <summary>
/// A utility script to verify that all SAO Mixed Reality components are correctly set up and communicating.
/// Attach this to a permanent object in the scene.
/// </summary>
public class SAOSystemSanityCheck : MonoBehaviour
{
    void Start()
    {
        Debug.Log("[SAO] --- System Sanity Check Started ---");

        // 1. Check ARSceneManager
        var arManager = FindObjectOfType<ARSceneManager>();
        if (arManager != null)
        {
            Debug.Log("[SAO] ARSceneManager found.");
            if (arManager.ovrSceneManager == null && arManager.GetComponent("OVRSceneManager") == null)
            {
                Debug.LogWarning("[SAO] ARSceneManager: OVRSceneManager dependency is missing!");
            }
        }
        else
        {
            Debug.LogError("[SAO] ARSceneManager NOT found in scene!");
        }

        // 2. Check EnemySpawnManager
        var spawnManager = FindObjectOfType<EnemySpawnManager>();
        if (spawnManager != null)
        {
            Debug.Log("[SAO] EnemySpawnManager found.");
            if (spawnManager.enemyPrefabs == null || spawnManager.enemyPrefabs.Length == 0)
            {
                Debug.LogWarning("[SAO] EnemySpawnManager: No enemy prefabs assigned!");
            }
        }
        else
        {
            Debug.LogError("[SAO] EnemySpawnManager NOT found in scene!");
        }

        // 3. Check PlayerStats
        var playerStats = FindObjectOfType<PlayerStats>();
        if (playerStats != null)
        {
            Debug.Log("[SAO] PlayerStats found.");
        }
        else
        {
            Debug.LogError("[SAO] PlayerStats NOT found in scene! Player won't have health/stamina.");
        }

        // 4. Check UIManager
        var uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null)
        {
            Debug.Log("[SAO] UIManager found.");
            if (uiManager.uiDocument == null) Debug.LogWarning("[SAO] UIManager: UIDocument is missing!");
            if (uiManager.hudTemplate == null) Debug.LogWarning("[SAO] UIManager: HUD Template not assigned.");
            if (uiManager.mainMenuTemplate == null) Debug.LogWarning("[SAO] UIManager: Main Menu Template not assigned.");
        }
        else
        {
            Debug.LogError("[SAO] UIManager NOT found in scene!");
        }

        // 5. Check Camera/Player
        if (Camera.main == null)
        {
            Debug.LogError("[SAO] Main Camera NOT found! AI and UI orientation will fail.");
        }
        else
        {
            Debug.Log($"[SAO] Main Camera found: {Camera.main.name}");
        }

        Debug.Log("[SAO] --- System Sanity Check Complete ---");
    }
}
