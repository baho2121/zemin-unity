using UnityEngine;

public class BreakableObject : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    public float currentHealth;
    public int coinReward = 10;

    [Header("Visuals")]
    public GameObject deathEffect; 
    public GameObject damageTextPrefab; // Assign an empty object with FloatingText on it, or we create simple ones

    private Vector3 initialScale;

    void Awake()
    {
        initialScale = transform.localScale;
    }

    void OnEnable()
    {
        currentHealth = maxHealth;
        // Reset scale in case it was pooled
        transform.localScale = initialScale; 
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        
        // Spawn Damage Text
        ShowDamageNumber(amount);
        
        // Simple hit effect: Tiny Punch
        transform.localScale = initialScale * 0.8f; 
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    void ShowDamageNumber(float amount)
    {
        // Simple Instantiation
        GameObject popup = new GameObject("DamagePopup");
        popup.transform.position = transform.position + Vector3.up * 1.5f; // Spawn above coin
        
        FloatingText ft = popup.AddComponent<FloatingText>();
        ft.Setup(amount);
    }

    void Update()
    {
        // Smooth return to INITIAL scale
        transform.localScale = Vector3.Lerp(transform.localScale, initialScale, Time.deltaTime * 5f);
    }

    void Die()
    {
        // Spawn effect
        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        // Notify global economy
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.AddCoins(coinReward);
        }
        else
        {
            Debug.Log("Gained " + coinReward + " Coins! (EconomyManager missing)");
        }

        // Destroy simple object
        Destroy(gameObject);
    }
}
