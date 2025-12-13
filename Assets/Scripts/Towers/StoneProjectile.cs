using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class StoneProjectile : MonoBehaviour
{
    private float lifeTime;
    private DamagePayload payload;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Initialize(Vector3 initialVelocity, float lifeTime, DamagePayload payload, float spin = 0f)
    {
        this.lifeTime = Mathf.Max(0.001f, lifeTime);
        this.payload = payload;

        rb.velocity = initialVelocity;

        if (spin != 0f)
            rb.angularVelocity = Random.onUnitSphere * spin;

        Destroy(gameObject, this.lifeTime);
    }

    void OnTriggerEnter(Collider other)
    {
        var health = other.GetComponentInParent<EnemyHealth>();
        if (health != null && !health.IsDead)
        {
            health.TakeDamage(payload.amount, payload.armorPenetration, payload.shred);

            var combat = other.GetComponentInParent<EnemyCombatController>();
            var movement = other.GetComponentInParent<EnemyMovement>();

            if (combat != null && movement != null && !movement.enabled)
            {
                Vector3 hitDir = (other.transform.position - transform.position).normalized;
                combat.PlayHitReaction(hitDir);
            }
        }
    }
}