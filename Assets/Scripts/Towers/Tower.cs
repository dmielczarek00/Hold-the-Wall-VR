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

    void Update()
    {
        EnemyMovement target = FindClosestEnemy();

        if (target != null)
        {
            Vector3 dir = (target.transform.position - movingPoint.position).normalized;
            Quaternion lookRot = Quaternion.LookRotation(dir);
            movingPoint.rotation = Quaternion.Slerp(movingPoint.rotation, lookRot, Time.deltaTime * 5);

            if (Time.time >= nextFireTime)
            {
                Shoot(target);
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
            float dist = Vector3.Distance(transform.position, e.transform.position);
            if (dist < minDist && dist <= range)
            {
                minDist = dist;
                closest = e;
            }
        }

        return closest;
    }

    void Shoot(EnemyMovement enemy)
    {
        var projObj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Projectile proj = projObj.GetComponent<Projectile>();
        if (proj != null)
        {
            // przewidywanie pozycji
            Vector3 enemyPos = enemy.transform.position;
            Vector3 enemyDir = (enemy.path.points[enemy.CurrentIndex].position - enemyPos).normalized;
            float distance = Vector3.Distance(firePoint.position, enemyPos);
            float travelTime = distance / proj.speed;

            Vector3 predictedPos = enemyPos + enemyDir * enemy.speed * travelTime;

            // rozrzut
            float spread = (1f - accuracy) * maxSpreadAngle;
            Quaternion spreadRot = Quaternion.Euler(
                Random.Range(-spread, spread),
                Random.Range(-spread, spread),
                0f
            );

            Vector3 dirWithSpread = spreadRot * (predictedPos - firePoint.position);

            proj.SetTarget(firePoint.position + dirWithSpread);
        }
    }
}