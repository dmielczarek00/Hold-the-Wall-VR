using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 3;
    public int currentHealth;
    public Animator animator;
    public string deathTrigger = "Death";
    public float deathLifetime = 1f;

    public bool IsDead { get; private set; }

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount, GameObject source = null)
    {
        if (IsDead) return;

        currentHealth -= amount;
        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        if (IsDead) return;
        IsDead = true;

        // wy³¹cz ruch
        var move = GetComponent<EnemyMovement>();
        if (move != null) move.enabled = false;

        // animacja œmierci
        if (animator != null && !string.IsNullOrEmpty(deathTrigger))
            animator.SetTrigger(deathTrigger);

        // usuñ po czasie
        Destroy(gameObject, deathLifetime);
    }
}