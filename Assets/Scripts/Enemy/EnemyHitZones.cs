using System.Collections.Generic;
using UnityEngine;

public enum EnemyHitZone
{
    Head,
    Torso,
    Body,
    Armor,
    Weapon
}

public class EnemyHitZones : MonoBehaviour
{
    [Header("Referencje")]
    public EnemyHealth health;
    public EnemyCombatController combat;
    public EnemyWeapon enemyWeapon;

    [Header("Hitboxy cia³a")]
    [Tooltip("Collider g³owy.")]
    public Collider headCollider;

    [Tooltip("Collider torsu.")]
    public Collider torsoCollider;

    [Tooltip("Pozosta³e collidery.")]
    public List<Collider> otherBodyColliders = new List<Collider>();

    // collidery pancerza z EnemyHealth
    private readonly HashSet<Collider> _armorColliders = new HashSet<Collider>();

    void Awake()
    {
        if (health == null)
            health = GetComponent<EnemyHealth>();

        if (combat == null)
            combat = GetComponent<EnemyCombatController>();

        if (enemyWeapon == null)
            enemyWeapon = GetComponentInChildren<EnemyWeapon>();

        BuildArmorColliderSet();
    }

    void BuildArmorColliderSet()
    {
        if (health == null) return;

        CollectArmorColliders(health.smallArmorPieces);
        CollectArmorColliders(health.mediumArmorPieces);
        CollectArmorColliders(health.bigArmorPieces);
    }

    void CollectArmorColliders(List<GameObject> pieces)
    {
        if (pieces == null) return;

        foreach (var go in pieces)
        {
            if (go == null) continue;

            var cols = go.GetComponentsInChildren<Collider>(true);
            foreach (var c in cols)
            {
                if (c != null)
                    _armorColliders.Add(c);
            }
        }
    }

    public bool TryResolveHit(Collider col, out EnemyHitZone zone)
    {
        zone = EnemyHitZone.Body;
        if (col == null) return false;

        // broñ przeciwnika – clash, bez obra¿eñ
        if (enemyWeapon != null && col.transform.IsChildOf(enemyWeapon.transform))
        {
            zone = EnemyHitZone.Weapon;
            return true;
        }

        // g³owa
        if (headCollider != null && col == headCollider)
        {
            zone = EnemyHitZone.Head;
            return true;
        }

        // tors
        if (torsoCollider != null && col == torsoCollider)
        {
            zone = EnemyHitZone.Torso;
            return true;
        }

        // reszta cia³a
        if (otherBodyColliders != null && otherBodyColliders.Contains(col))
        {
            zone = EnemyHitZone.Body;
            return true;
        }

        // pancerz
        if (_armorColliders.Contains(col))
        {
            zone = EnemyHitZone.Armor;
            return true;
        }

        zone = EnemyHitZone.Body;
        return true;
    }
}