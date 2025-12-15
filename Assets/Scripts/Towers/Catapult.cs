using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Catapult : MonoBehaviour
{
    [Header("Dane konfiguracyjne")]
    public TowerData data;

    [Header("Referencje scenowe")]
    public Transform movingPoint;
    public Transform firePoint;

    [Header("Animacja")]
    public Animator animator;
    public GameObject stoneVisual;

    private float nextFireTime = 0f;

    private EnemyMovement pendingEnemy;
    private Vector3 pendingAimPosNow;
    private float pendingBulletSpeed;

    private bool waitingForShot = false;

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

            if (Time.time >= nextFireTime && !waitingForShot)
            {
                PrepareShoot(target, aimPos, bulletSpeed);

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
        Vector3 enemyDir = toWaypoint.sqrMagnitude > 0.0001f ? toWaypoint.normalized : Vector3.zero;
        Vector3 v = enemyDir * enemy.speed;

        float s = Mathf.Max(0.001f, bulletSpeed);

        // pierwszy przybli¿ony czas
        float t = r.magnitude / s;

        // licz dystans do pozycji po t
        Vector3 r2 = r + v * t;
        t = r2.magnitude / s;

        return aimPosNow + v * t;
    }

    // w³¹cza animacjê i czeka na event
    void PrepareShoot(EnemyMovement enemy, Vector3 aimPosNow, float bulletSpeed)
    {
        pendingEnemy = enemy;
        pendingAimPosNow = aimPosNow;
        pendingBulletSpeed = bulletSpeed;

        waitingForShot = true;

        if (animator != null)
            animator.SetTrigger("Fire");
        else
            HideStone();
    }

    public void HideStone()
    {
        if (stoneVisual != null)
            stoneVisual.SetActive(false);

        Shoot(pendingEnemy, pendingAimPosNow, pendingBulletSpeed);
    }

    public void ShowStone()
    {
        if (stoneVisual != null)
            stoneVisual.SetActive(true);

        waitingForShot = false;
    }

    void Shoot(EnemyMovement enemy, Vector3 aimPosNow, float bulletSpeed)
    {
        // odœwie¿a cel  przed strza³em
        if (enemy == null || !enemy.enabled || !enemy.gameObject.activeInHierarchy)
            enemy = FindClosestEnemy();

        if (enemy != null)
        {
            if (enemy.path == null || enemy.path.points == null)
                enemy = FindClosestEnemy();
            else if (enemy.CurrentIndex < 0 || enemy.CurrentIndex >= enemy.path.points.Length)
                enemy = FindClosestEnemy();
        }

        if (enemy != null)
        {
            aimPosNow = enemy.aimPoint != null
                ? enemy.aimPoint.position
                : enemy.transform.position;
        }

        var projObj = Instantiate(data.projectilePrefab, firePoint.position, firePoint.rotation);
        var proj = projObj.GetComponent<StoneProjectile>();
        if (proj == null) return;

        Vector3 predictedPos = (enemy != null)
            ? ComputePredictedPos(enemy, aimPosNow, data.projectileSpeed)
            : (firePoint.position + firePoint.forward * 5f);

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

        Vector3 v0 = GetBallisticVelocityLowArc(firePoint.position, finalAimPoint, data.projectileSpeed);
        proj.Initialize(v0, data.projectileLifeTime, payload, 0f);
    }

    // obliczanie trajektorii lotu
    Vector3 GetBallisticVelocityLowArc(Vector3 origin, Vector3 target, float launchSpeed)
    {
        float gravity = Mathf.Abs(Physics.gravity.y);

        Vector3 toTarget = target - origin;

        // dystans w poziomie
        Vector3 toTargetFlat = new Vector3(toTarget.x, 0f, toTarget.z);
        float horizontalDistance = toTargetFlat.magnitude;

        // ró¿nica wysokoœci
        float heightDifference = toTarget.y;

        if (horizontalDistance < 0.001f)
            return Vector3.up * launchSpeed;

        float speedSquared = launchSpeed * launchSpeed;
        float speedToFourth = speedSquared * speedSquared;

        // sprawdzenie czy da siê trafiæ
        float discriminant =
            speedToFourth -
            gravity * (gravity * horizontalDistance * horizontalDistance
                       + 2f * heightDifference * speedSquared);

        // jeœli nie da siê trafiæ
        if (discriminant < 0f)
            return toTarget.normalized * launchSpeed;

        float sqrtDiscriminant = Mathf.Sqrt(discriminant);

        float tangentAngle =
            (speedSquared - sqrtDiscriminant) /
            (gravity * horizontalDistance);

        float launchAngle = Mathf.Atan(tangentAngle);

        Vector3 horizontalDir = toTargetFlat.normalized;

        float cos = Mathf.Cos(launchAngle);
        float sin = Mathf.Sin(launchAngle);

        return horizontalDir * (launchSpeed * cos)
             + Vector3.up * (launchSpeed * sin);
    }
}