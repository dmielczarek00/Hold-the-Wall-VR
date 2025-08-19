using UnityEngine;

[CreateAssetMenu(menuName = "TD/Tower", fileName = "TowerData")]
public class TowerData : ScriptableObject
{
    [Header("Meta")]
    public string displayName;
    public Sprite icon;
    public GameObject prefab;
    [Header("Opis")]
    [TextArea(2, 4)] public string description;

    [Header("Ekonomia")]
    public int cost = 50;
    [Range(0f, 1f)] public float sellPercent = 0.8f;

    [Header("Statystyki wie¿y")]
    public float range = 10f;
    public float fireRate = 1f;
    public float armorPenetration = 2f;
    public float shred = 1f;
    [Range(0f, 1f)] public float accuracy = 1f;
    public float maxSpreadAngle = 5f;

    [Header("Obra¿enia")]
    public int damage = 1;

    [Header("Pocisk")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 15f;
    public float projectileLifeTime = 5f;

    [Header("Upgrade")]
    public TowerData upgradeTo;
    public int upgradeCost = 100;
}