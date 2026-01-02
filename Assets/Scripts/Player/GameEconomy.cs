using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameEconomy : MonoBehaviour
{
    public static GameEconomy I;

    public int money = 200;

    public int totalEarnedMoney = 0;

    [Header("Zdarzenia")]
    public UnityEvent onMoneyChanged;

    [Header("Enemy count test")]
    public float enemyCountInterval = 1f;

    void Awake()
    {
        I = this;
        onMoneyChanged?.Invoke();
    }

    void Start()
    {
        StartCoroutine(CountEnemiesRoutine());
    }

    IEnumerator CountEnemiesRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(enemyCountInterval);

            int currentEnemyCount = FindObjectsOfType<EnemyHealth>().Length;

            if (currentEnemyCount > totalEarnedMoney)
            {
                totalEarnedMoney = currentEnemyCount;
            }
        }
    }

    public bool CanAfford(int amount) => money >= amount;

    public bool TrySpend(int amount)
    {
        if (money < amount) return false;
        money -= amount;
        onMoneyChanged?.Invoke();
        return true;
    }

    public void Add(int amount)
    {
        money += amount;

        // STARA LOGIKA – tymczasowo wy³¹czona
        // totalEarnedMoney += amount;

        onMoneyChanged?.Invoke();
    }
}