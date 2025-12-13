using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    private float speed;
    private float lifeTime;
    private Vector3 direction;

    private DamagePayload payload;

    public void Initialize(Vector3 targetPosition, float speed, float lifeTime, DamagePayload payload)
    {
        this.speed = Mathf.Max(0.001f, speed);
        this.lifeTime = Mathf.Max(0.001f, lifeTime);
        this.payload = payload;

        direction = (targetPosition - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);

        Destroy(gameObject, this.lifeTime);
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        var health = other.GetComponentInParent<EnemyHealth>();
        if (health != null)
        {
            health.TakeDamage(payload.amount, payload.armorPenetration, payload.shred);

            var combat = other.GetComponentInParent<EnemyCombatController>();
            var movement = other.GetComponentInParent<EnemyMovement>();

            if (combat != null && movement != null && !movement.enabled)
            {
                Vector3 hitDir = (other.transform.position - transform.position).normalized;
                combat.PlayHitReaction(hitDir);
            }

            Destroy(gameObject);
            return;
        }

        Destroy(gameObject);
    }
}