using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Życie")]
    public int maxHealth = 3;
    public int currentHealth;

    [Header("Pancerz")]
    public int maxArmor = 0;
    public int currentArmor;

    [Header("Nagroda za zabicie")]
    public int moneyReward = 10;

    // proporcje pancerza
    [Range(0, 100)] public int smallArmorShare = 20;
    [Range(0, 100)] public int mediumArmorShare = 20;
    [Range(0, 100)] public int bigArmorShare = 60;

    // fizyczne części pancerza
    public List<GameObject> smallArmorPieces = new List<GameObject>();
    public List<GameObject> mediumArmorPieces = new List<GameObject>();
    public List<GameObject> bigArmorPieces = new List<GameObject>();

    // punkty armora przypisane do segmentów
    private int smallArmorPoints;
    private int mediumArmorPoints;
    private int bigArmorPoints;

    // ile części już odpadło
    private int smallArmorRemoved = 0;
    private int mediumArmorRemoved = 0;
    private int bigArmorRemoved = 0;

    public Animator animator;
    public string deathTrigger = "Death";
    public float deathLifetime = 1f;

    public bool IsDead { get; private set; }

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] hitSounds;
    [SerializeField] private AudioClip[] deathSounds;

    void Awake()
    {
        InitStats();
    }

    public void ApplyMultipliers(float hpMul, float armorMul, float goldMul)
    {
        if (hpMul <= 0f) hpMul = 1f;
        if (armorMul <= 0f) armorMul = 1f;
        if (goldMul <= 0f) goldMul = 1f;

        int newHp = Mathf.Max(1, Mathf.RoundToInt(maxHealth * hpMul));
        int newArmor = Mathf.Max(0, Mathf.RoundToInt(maxArmor * armorMul));
        int newGold = Mathf.Max(0, Mathf.RoundToInt(moneyReward * goldMul));

        maxHealth = newHp;
        maxArmor = newArmor;
        moneyReward = newGold;

        InitStats();
    }

    private void InitStats()
    {
        currentHealth = maxHealth;
        currentArmor = maxArmor;

        // przypisanie proporcji do faktycznej ilości pancerza
        smallArmorPoints = Mathf.RoundToInt(maxArmor * (smallArmorShare / 100f));
        mediumArmorPoints = Mathf.RoundToInt(maxArmor * (mediumArmorShare / 100f));
        bigArmorPoints = maxArmor - smallArmorPoints - mediumArmorPoints;
    }

    public void TakeDamage(int damage, int armorPenetration, int shred = 0)
    {
        if (IsDead) return;

        // obliczanie penetracji pancerza
        int effectiveArmor = Mathf.Max(0, currentArmor - Mathf.Max(0, armorPenetration));

        // finalne obrażenia
        int finalDamage = Mathf.Max(0, damage - effectiveArmor);
        AudioPlay.PlaySound(audioSource, hitSounds);
        currentHealth -= finalDamage;

        // obliczanie ściągania pancerza
        if (shred > 0 && currentArmor > 0)
        {
            currentArmor = Mathf.Max(0, currentArmor - shred);
            UpdateArmorPieces();
        }

        if (currentHealth <= 0)
        {
            AudioPlay.PlaySound(audioSource, deathSounds);
            Die();
        }
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

        // dodaj złoto
        if (GameEconomy.I != null)
        {
            GameEconomy.I.Add(moneyReward);
        }

        // wyłącz ruch
        var move = GetComponent<EnemyMovement>();
        if (move != null) move.enabled = false;

        // wyłącz collider
        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // animacja śmierci
        if (animator != null && !string.IsNullOrEmpty(deathTrigger))
            animator.SetTrigger(deathTrigger);

        // usuń po czasie
        Destroy(gameObject, deathLifetime);
    }
}