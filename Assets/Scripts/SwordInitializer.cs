using UnityEngine;

public class SwordInitializer : MonoBehaviour
{
    public GameObject swordMenuPrefab;
    public GameObject inventoryManagerPrefab;

    void Awake()
    {
        EnsureInventoryManager();
        EnsureSwordMenu();
        
        Debug.Log("[SAO] SwordInitializer completed setup.");
    }

    void EnsureInventoryManager()
    {
        if (PlayerInventoryManager.Instance == null)
        {
            if (inventoryManagerPrefab != null)
            {
                Instantiate(inventoryManagerPrefab);
                Debug.Log("[SAO] Instantiated PlayerInventoryManager from prefab.");
            }
            else
            {
                GameObject go = new GameObject("PlayerInventoryManager");
                go.AddComponent<PlayerInventoryManager>();
                Debug.Log("[SAO] Created new PlayerInventoryManager GameObject.");
            }
        }
    }

    void EnsureSwordMenu()
    {
        SwordMenuManager menu = FindObjectOfType<SwordMenuManager>();
        if (menu == null)
        {
            if (swordMenuPrefab != null)
            {
                Instantiate(swordMenuPrefab);
                Debug.Log("[SAO] Instantiated SwordMenuManager from prefab.");
            }
            else
            {
                GameObject go = new GameObject("SwordMenuManager");
                go.AddComponent<SwordMenuManager>();
                Debug.Log("[SAO] Created new SwordMenuManager GameObject.");
            }
        }
    }
}