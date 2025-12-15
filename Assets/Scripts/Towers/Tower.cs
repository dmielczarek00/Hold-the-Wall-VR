using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("Dane konfiguracyjne")]
    public TowerData data;

    [Header("Referencje scenowe")]
    public Transform movingPoint;
    public Transform firePoint;

    [Header("DŸwiêki")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] fireSounds;

    private float nextFireTime = 0f;

    void Update()
    {

        EnemyMovement target = FindClosestEnemy();

        if (target != null)
        {
            // Celowanie w AimPoint (jeœli jest)
            Vector3 aimPos = target.aimPoint != null
                ? target.aimPoint.position
                : target.transform.position;

            Vector3 dir = (aimPos - movingPoint.position).normalized;
            Quaternion lookRot = Quaternion.LookRotation(dir);
            movingPoint.rotation = Quaternion.Slerp(movingPoint.rotation, lookRot, Time.deltaTime * 5);

            // Podgl¹d linii w kierunku przewidywanego celu
            float bulletSpeed = Mathf.Max(0.001f, data.projectileSpeed);
            Vector3 predictedPosForDebug = ComputePredictedPos(target, aimPos, bulletSpeed);
            Debug.DrawLine(firePoint.position, predictedPosForDebug, Color.red, 0f, false);

            if (Time.time >= nextFireTime)
            {
                Shoot(target, aimPos, bulletSpeed);
                nextFireTime = Time.time + 1f / Mathf.Max(0.0001f, data.fireRate);
            }
        }
    }

    EnemyMovement FindClosestEnemy()
    {
        float useRange = data != null ? data.range : 0f;

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

            if (e.path == null || e.path.points == null)
                continue;

            if (e.CurrentIndex < 0 || e.CurrentIndex >= e.path.points.Length)
                continue;

            float dist = Vector3.Distance(transform.position, e.transform.position);
            if (dist < minDist && dist <= useRange)
            {
                minDist = dist;
                closest = e;
            }
        }

        return closest;
    }

    // przewidywanie pozycji przeciwnika
    Vector3 ComputePredictedPos(EnemyMovement enemy, Vector3 aimPosNow, float bulletSpeed)
    {
        if (enemy == null ||
            enemy.path == null ||
            enemy.path.points == null ||
            enemy.CurrentIndex < 0 ||
            enemy.CurrentIndex >= enemy.path.points.Length)
        {
            return aimPosNow;
        }

        Vector3 r = aimPosNow - firePoint.position;

        // kierunek ruchu wroga
        Vector3 bottomNow = enemy.transform.position + Vector3.down * (enemy.enemyHeight * 0.5f);
        Vector3 toWaypoint = enemy.path.points[enemy.CurrentIndex].position - bottomNow;
        toWaypoint.y = 0f;

        Vector3 enemyDir = toWaypoint.sqrMagnitude > 0.0001f
            ? toWaypoint.normalized
            : Vector3.zero;

        Vector3 v = enemyDir * enemy.speed;

        float s = Mathf.Max(0.001f, bulletSpeed);

        // pierwszy przybli¿ony czas
        float t = r.magnitude / s;

        // licz dystans do pozycji po t
        Vector3 r2 = r + v * t;
        t = r2.magnitude / s;

        return aimPosNow + v * t;
    }

    void Shoot(EnemyMovement enemy, Vector3 aimPosNow, float bulletSpeed)
    {
        var projObj = Instantiate(data.projectilePrefab, firePoint.position, firePoint.rotation);
        var proj = projObj.GetComponent<Projectile>();
        if (proj == null) return;

        Vector3 predictedPos = ComputePredictedPos(enemy, aimPosNow, data.projectileSpeed);

        // rozrzut
        float spread = (1f - Mathf.Clamp01(data.accuracy)) * data.maxSpreadAngle;
        Quaternion spreadRot = Quaternion.Euler(
            Random.Range(-spread, spread),
            Random.Range(-spread, spread),
            0f
        );

        Vector3 dirWithSpread = spreadRot * (predictedPos - firePoint.position);
        Vector3 finalAimPoint = firePoint.position + dirWithSpread;

        var payload = new DamagePayload
        {
            amount = Mathf.Max(0, data.damage),
            armorPenetration = Mathf.RoundToInt(data.armorPenetration),
            shred = Mathf.RoundToInt(data.shred),
            source = gameObject
        };

        AudioPlay.PlaySound(audioSource, fireSounds);
        proj.Initialize(finalAimPoint, data.projectileSpeed, data.projectileLifeTime, payload);
    }
}