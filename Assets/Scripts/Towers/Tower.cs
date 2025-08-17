using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    public Transform movingPoint;
    public Transform firePoint;
    public GameObject projectilePrefab;

    public float range = 10f;
    public float fireRate = 1f;
    private float nextFireTime = 0f;

    [Header("Celnoœæ")]
    [Range(0f, 1f)]
    public float accuracy = 1f;       // 1 = idealnie celna, 0 = bardzo niecelna
    public float maxSpreadAngle = 5f; // maksymalny rozrzut

    [Header("Obra¿enia z wie¿y")]
    public int baseDamage = 1;
    public int damagePerLevel = 1;
    public int level = 1;
    public float damageMultiplier = 1f;

    [Header("Prêdkoœæ pocisku")]
    public float baseProjectileSpeed = 15f;
    public float projectileSpeedPerLevel = 0f;

    void Update()
    {
        EnemyMovement target = FindClosestEnemy();

        if (target != null)
        {
            // celuje w AimPoint
            Vector3 aimPos = target.aimPoint != null
                ? target.aimPoint.position
                : target.transform.position;

            Vector3 dir = (aimPos - movingPoint.position).normalized;
            Quaternion lookRot = Quaternion.LookRotation(dir);
            movingPoint.rotation = Quaternion.Slerp(movingPoint.rotation, lookRot, Time.deltaTime * 5);

            // resuje liniê tam gdzie wie¿a celuje
            float bulletSpeed = CalculateProjectileSpeed();
            Vector3 predictedPosForDebug = ComputePredictedPos(target, aimPos, bulletSpeed);
            Debug.DrawLine(firePoint.position, predictedPosForDebug, Color.red, 0f, false);

            if (Time.time >= nextFireTime)
            {
                Shoot(target, aimPos, bulletSpeed);
                nextFireTime = Time.time + 1f / fireRate;
            }
        }
    }

    EnemyMovement FindClosestEnemy()
    {
        EnemyMovement[] enemies = FindObjectsOfType<EnemyMovement>();
        EnemyMovement closest = null;
        float minDist = Mathf.Infinity;

        foreach (var e in enemies)
        {
            if (e == null || !e.enabled || !e.gameObject.activeInHierarchy)
                continue;

            var hp = e.GetComponent<EnemyHealth>();
            if (hp != null && hp.IsDead)
                continue;

            float dist = Vector3.Distance(transform.position, e.transform.position);
            if (dist < minDist && dist <= range)
            {
                minDist = dist;
                closest = e;
            }
        }

        return closest;
    }

    // kalkulacja obra¿eñ
    int CalculateDamage()
    {
        float scaled = (baseDamage + (level - 1) * damagePerLevel) * damageMultiplier;
        return Mathf.Max(0, Mathf.RoundToInt(scaled));
    }

    // kalkulacja prêdkoœci pocisku
    float CalculateProjectileSpeed()
    {
        float scaled = (baseProjectileSpeed + (level - 1) * projectileSpeedPerLevel);
        return Mathf.Max(0.001f, scaled);
    }

    // przewidywanie pozycji przeciwnika
    Vector3 ComputePredictedPos(EnemyMovement enemy, Vector3 aimPosNow, float bulletSpeed)
    {
        Vector3 bottomNow = enemy.transform.position + Vector3.down * (enemy.enemyHeight * 0.5f);
        Vector3 toWaypoint = enemy.path.points[enemy.CurrentIndex].position - bottomNow;
        toWaypoint.y = 0f;

        Vector3 enemyDir = toWaypoint.sqrMagnitude > 0.0001f ? toWaypoint.normalized : Vector3.zero;

        float distance = Vector3.Distance(firePoint.position, aimPosNow);
        float travelTime = distance / Mathf.Max(0.001f, bulletSpeed);

        return aimPosNow + enemyDir * enemy.speed * travelTime;
    }

    void Shoot(EnemyMovement enemy, Vector3 aimPosNow, float bulletSpeed)
    {
        var projObj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Projectile proj = projObj.GetComponent<Projectile>();
        if (proj == null) return;

        // przewidywanie pozycji przeciwnika
        Vector3 predictedPos = ComputePredictedPos(enemy, aimPosNow, bulletSpeed);

        // rozrzut
        float spread = (1f - accuracy) * maxSpreadAngle;
        Quaternion spreadRot = Quaternion.Euler(
            Random.Range(-spread, spread),
            Random.Range(-spread, spread),
            0f
        );

        Vector3 dirWithSpread = spreadRot * (predictedPos - firePoint.position);
        Vector3 finalAimPoint = firePoint.position + dirWithSpread;

        // ustaw cel i prêdkoœæ pocisku
        proj.SetTarget(finalAimPoint);
        proj.SetSpeed(bulletSpeed);

        // payload z obra¿eniami z wie¿y
        var payload = new DamagePayload
        {
            amount = CalculateDamage(),
            source = gameObject
        };
        proj.SetPayload(payload);
    }
}