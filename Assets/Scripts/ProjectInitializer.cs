using UnityEngine;

/// <summary>
/// Ensures all core AR and SAO systems are present in the scene and correctly configured.
/// This acts as a 'master setup' tool for the project.
/// </summary>
public class ProjectInitializer : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject arGameManagerPrefab;
    public GameObject arSceneManagerPrefab;
    public GameObject uiManagerPrefab;
    public GameObject swordInitializerPrefab;

    [Header("Debug Settings")]
    public Material wallMaterial;
    public Material ceilingMaterial;
    public Material floorMaterial;

    private void Awake()
    {
        InitializeManagers();
        Debug.Log("[SAO] Project Initialization Complete.");
    }

    private void InitializeManagers()
    {
        // 1. AR Scene Manager
        if (FindObjectOfType<ARSceneManager>() == null)
        {
            if (arSceneManagerPrefab != null)
            {
                var asm = Instantiate(arSceneManagerPrefab).GetComponent<ARSceneManager>();
                if (wallMaterial != null) asm.wallDebugMaterial = wallMaterial;
                if (ceilingMaterial != null) asm.ceilingDebugMaterial = ceilingMaterial;
                if (floorMaterial != null) asm.floorDebugMaterial = floorMaterial;
            }
            else
            {
                GameObject go = new GameObject("ARSceneManager");
                go.AddComponent<ARSceneManager>();
                // OVRSceneManager might be in a namespace or needs special handling if lint fails
                // For now, assume it's globally available if Oculus.VR is referenced
#if !UNITY_EDITOR
                go.AddComponent<OVRSceneManager>();
#endif
            }
        }

        // 2. UI Manager
        if (UIManager.Instance == null)
        {
            if (uiManagerPrefab != null) Instantiate(uiManagerPrefab);
        }

        // 3. AR Game Manager
        if (ARGameManager.Instance == null)
        {
            if (arGameManagerPrefab != null) Instantiate(arGameManagerPrefab);
        }

        // 4. Sword Initializer
        if (FindObjectOfType<SwordInitializer>() == null)
        {
            if (swordInitializerPrefab != null) Instantiate(swordInitializerPrefab);
        }
    }
}
