using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [Header("Ustawienia HP")]
    [Tooltip("Zdrowie gracza na starcie.")]
    public float maxHealth = 100f;

    [Tooltip("Aktualne zdrowie.")]
    public float currentHealth;

    [Header("Zdarzenia")]
    public UnityEvent onDamaged;

    public UnityEvent onDeath;

    private bool _isDead = false;

    private void Awake()
    {
        // Startowe HP
        currentHealth = maxHealth;
    }

    /// Zadaje obra¿enia
    public void TakeDamage(float amount)
    {
        if (_isDead) return;
        if (amount <= 0f) return;

        currentHealth -= amount;
        if (currentHealth < 0f)
            currentHealth = 0f;

        // Event po otrzymaniu obra¿eñ
        onDamaged?.Invoke();

        // Sprawdzenie œmierci
        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    /// Reset HP
    public void ResetHealth()
    {
        _isDead = false;
        currentHealth = maxHealth;
    }

    private void Die()
    {
        if (_isDead) return;

        _isDead = true;
        onDeath?.Invoke();
    }
}