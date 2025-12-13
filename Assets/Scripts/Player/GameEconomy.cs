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

    void Awake()
    {
        I = this;
        onMoneyChanged?.Invoke();
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
        totalEarnedMoney += amount;
        onMoneyChanged?.Invoke();
    }
}