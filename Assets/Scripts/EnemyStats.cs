using UnityEngine;
using UnityEngine.UI;

public class EnemyStats : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 50f;
    public float currentHealth;
    public Image healthBarFill;
    public Canvas healthBarCanvas;

    public bool IsDead => currentHealth <= 0;

    private Animator _animator;
    private bool _isDying = false;

    private void Start()
    {
        currentHealth = maxHealth;
        _animator = GetComponent<Animator>();
        UpdateUI();
    }

    private void Update()
    {
        if (healthBarCanvas != null && Camera.main != null)
        {
            healthBarCanvas.transform.LookAt(Camera.main.transform);
            healthBarCanvas.transform.Rotate(0, 180, 0);
        }
    }

    public void TakeDamage(float amount)
    {
        if (_isDying) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);
        UpdateUI();

        if (currentHealth > 0)
        {
            if (_animator != null && _animator.HasState(0, Animator.StringToHash("GetHit")))
            {
                _animator.CrossFade("GetHit", 0.1f);
            }
        }
        else
        {
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.AddScore(100);
            }
            Die();
        }
    }

    private void UpdateUI()
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = currentHealth / maxHealth;
        }
    }

    private void Die()
    {
        if (_isDying) return;
        _isDying = true;

        Debug.Log($"[SAO] Enemy {gameObject.name} defeated!");

        // Disable colliders so it doesn't block attacks or movement
        var colliders = GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }

        if (healthBarCanvas != null)
        {
            healthBarCanvas.gameObject.SetActive(false);
        }

        if (_animator != null && _animator.HasState(0, Animator.StringToHash("Die")))
        {
            _animator.CrossFade("Die", 0.1f);
            Destroy(gameObject, 1.5f);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
