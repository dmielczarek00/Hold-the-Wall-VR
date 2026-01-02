using UnityEngine;
using TMPro;

public class StatsDisplay : MonoBehaviour
{
    [Header("Referencje")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text moneyText;

    private void Start()
    {
        RefreshMoney();
        RefreshHealth();
    }
    public void RefreshHealth()
    {
        if (healthText == null || playerHealth == null) return;
        healthText.text = playerHealth.currentHealth + " HP";
    }
    public void RefreshMoney()
    {
        if (moneyText == null || GameEconomy.I == null) return;
        moneyText.text = GameEconomy.I.money + " Gold";
    }
}