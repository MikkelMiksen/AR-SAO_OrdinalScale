using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Stamina")]
    public float maxStamina = 100f;
    public float currentStamina;
    public float staminaRegenRate = 10f;

    [Header("Combat Settings")]
    public float attackStaminaCost = 20f;
    public float swordDamage = 25f;

    public PhysicsMaterial playerPhysicsMaterial;

    public int score = 0;

    public static PlayerStats Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;

        // Apply physics material to player colliders
        if (playerPhysicsMaterial != null)
        {
            var colliders = GetComponentsInChildren<Collider>();
            foreach (var col in colliders)
            {
                col.material = playerPhysicsMaterial;
            }
        }

        UpdateUI();
    }

    private void Update()
    {
        if (currentStamina < maxStamina)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            currentStamina = Mathf.Min(currentStamina, maxStamina);
            UpdateUI();
        }
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateUI();
    }

    public bool CanAttack()
    {
        return currentStamina >= attackStaminaCost;
    }

    public bool CanAttack(float cost)
    {
        return currentStamina >= cost;
    }

    public void UseStaminaForAttack()
    {
        UseStamina(attackStaminaCost);
    }

    public void UseStamina(float amount)
    {
        currentStamina -= amount;
        currentStamina = Mathf.Max(currentStamina, 0);
        UpdateUI();
    }

    public void ResetStats()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        score = 0;
        UpdateUI();
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);
        UpdateUI();
        
        if (currentHealth <= 0)
        {
            Debug.Log("[SAO] Player Died!");
            if (ARGameManager.Instance != null)
            {
                ARGameManager.Instance.SetState(GameState.GameOver);
            }
        }
    }

    private void UpdateUI()
    {
        if (UIManager.Instance != null)
        {
            int wave = ARGameManager.Instance != null ? ARGameManager.Instance.currentWave : 0;
            int remaining = ARGameManager.Instance != null && ARGameManager.Instance.enemySpawnManager != null ? 
                ARGameManager.Instance.enemySpawnManager.GetRemainingEnemiesCount() : 0;
            
            UIManager.Instance.UpdateHUD(currentHealth / maxHealth, currentStamina / maxStamina, wave, remaining, score);
        }
    }
}
