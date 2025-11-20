using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("¯ycie")]
    public int maxHealth = 3;
    public int currentHealth;

    [Header("Pancerz")]
    public int maxArmor = 0;
    public int currentArmor;

    // proporcje pancerza
    [Range(0, 100)] public int smallArmorShare = 20;
    [Range(0, 100)] public int mediumArmorShare = 20;
    [Range(0, 100)] public int bigArmorShare = 60;

    // fizyczne czêœci pancerza
    public List<GameObject> smallArmorPieces = new List<GameObject>();
    public List<GameObject> mediumArmorPieces = new List<GameObject>();
    public List<GameObject> bigArmorPieces = new List<GameObject>();

    // punkty armora przypisane do segmentów
    private int smallArmorPoints;
    private int mediumArmorPoints;
    private int bigArmorPoints;

    // ile czêœci ju¿ odpad³o
    private int smallArmorRemoved = 0;
    private int mediumArmorRemoved = 0;
    private int bigArmorRemoved = 0;

    public Animator animator;
    public string deathTrigger = "Death";
    public float deathLifetime = 1f;

    public bool IsDead { get; private set; }

    void Awake()
    {
        currentHealth = maxHealth;
        currentArmor = maxArmor;

        // przypisanie proporcji do faktycznej iloœci pancerza
        smallArmorPoints = Mathf.RoundToInt(maxArmor * (smallArmorShare / 100f));
        mediumArmorPoints = Mathf.RoundToInt(maxArmor * (mediumArmorShare / 100f));
        bigArmorPoints = maxArmor - smallArmorPoints - mediumArmorPoints;
    }

    public void TakeDamage(int damage, int armorPenetration, int shred = 0)
    {
        if (IsDead) return;

        // obliczanie penetracji pancerza
        int effectiveArmor = Mathf.Max(0, currentArmor - Mathf.Max(0, armorPenetration));

        // finalne obra¿enia
        int finalDamage = Mathf.Max(0, damage - effectiveArmor);
        currentHealth -= finalDamage;

        // obliczanie œci¹gania pancerza
        if (shred > 0 && currentArmor > 0)
        {
            currentArmor = Mathf.Max(0, currentArmor - shred);
            UpdateArmorPieces();
        }

        if (currentHealth <= 0) Die();
    }

    private void UpdateArmorPieces()
    {
        int armorLost = maxArmor - currentArmor;

        int destroyedSmall = (armorLost >= smallArmorPoints) ? smallArmorPieces.Count : 0;
        int destroyedMedium = (armorLost >= smallArmorPoints + mediumArmorPoints) ? mediumArmorPieces.Count : 0;
        int destroyedBig = (armorLost >= smallArmorPoints + mediumArmorPoints + bigArmorPoints) ? bigArmorPieces.Count : 0;

        RemovePiecesToTarget(smallArmorPieces, ref smallArmorRemoved, destroyedSmall);
        RemovePiecesToTarget(mediumArmorPieces, ref mediumArmorRemoved, destroyedMedium);
        RemovePiecesToTarget(bigArmorPieces, ref bigArmorRemoved, destroyedBig);
    }

    private void RemovePiecesToTarget(List<GameObject> pieces, ref int removedCount, int targetRemoved)
    {
        while (removedCount < targetRemoved && pieces.Count > 0)
        {
            int index = Random.Range(0, pieces.Count);
            if (pieces[index] != null && pieces[index].activeSelf)
            {
                pieces[index].SetActive(false);
                removedCount++;
            }
        }
    }

    public void TakeBodyDamage(int damage)
    {
        if (IsDead) return;
        if (damage <= 0) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }


    void Die()
    {
        if (IsDead) return;
        IsDead = true;

        // wy³¹cz ruch
        var move = GetComponent<EnemyMovement>();
        if (move != null) move.enabled = false;

        // wy³¹cz collider
        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // animacja œmierci
        if (animator != null && !string.IsNullOrEmpty(deathTrigger))
            animator.SetTrigger(deathTrigger);

        // usuñ po czasie
        Destroy(gameObject, deathLifetime);
    }
}