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
    public UnityEvent onHealthChanged;

    [Header("DŸwiêki")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] hurtSounds;

    private bool _isDead = false;

    private void Awake()
    {
        currentHealth = maxHealth;
        onHealthChanged?.Invoke();
    }

    /// Zadaje obra¿enia
    public void TakeDamage(float amount)
    {
        if (_isDead) return;
        if (amount <= 0f) return;

        currentHealth -= amount;
        if (currentHealth < 0f)
            currentHealth = 0f;

        onDamaged?.Invoke();
        onHealthChanged?.Invoke();
        AudioPlay.PlaySound(audioSource, hurtSounds);

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
        onHealthChanged?.Invoke();
    }

    private void Die()
    {
        if (_isDead) return;

        _isDead = true;
        onDeath?.Invoke();
    }
}