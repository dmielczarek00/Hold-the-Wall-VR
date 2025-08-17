using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEconomy : MonoBehaviour
{
    public static GameEconomy I;
    public int money = 200;

    void Awake() { I = this; }

    public bool CanAfford(int amount) => money >= amount;

    public bool TrySpend(int amount)
    {
        if (money < amount) return false;
        money -= amount;
        return true;
    }

    public void Add(int amount) { money += amount; }
}