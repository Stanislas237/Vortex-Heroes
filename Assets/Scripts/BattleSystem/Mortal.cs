using UnityEngine;
using UnityEngine.UI;

public class Mortal : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;
    public GameObject deathEffect;
    public Image healthBar; // Optionnel si tu veux une UI attachée à l’objet

    void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        UpdateUI();

        if (currentHealth <= 0)
            Die();
    }

    public void TakeDamage() => Die();

    void UpdateUI()
    {
        if (healthBar != null)
            healthBar.fillAmount = Mathf.Clamp01((float)currentHealth / maxHealth);
    }

    void Die()
    {
        if (gameObject.CompareTag("Player"))
            foreach (var obstacle in ObstacleSpawner.obstacles)
                obstacle?.StopAllMoves();
        
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform).transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            Destroy(gameObject, .3f);
        }
    }
}
