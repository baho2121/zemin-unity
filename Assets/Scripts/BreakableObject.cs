using UnityEngine;

public class BreakableObject : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    public float currentHealth;
    public int coinReward = 10;

    [Header("Visuals")]
    public GameObject deathEffect; // Particle to spawn on death

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
        
        // Simple hit effect: Tiny Punch
        transform.localScale = initialScale * 0.8f; 
        
        if (currentHealth <= 0)
        {
            Die();
        }
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
