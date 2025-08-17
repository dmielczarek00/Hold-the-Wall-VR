using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    private float speed = 15f;
    private Vector3 direction;

    private DamagePayload payload;

    public void SetSpeed(float s) => speed = Mathf.Max(0.001f, s);
    public void SetPayload(DamagePayload p) => payload = p;

    public void SetTarget(Vector3 targetPosition)
    {
        direction = (targetPosition - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        var health = other.GetComponent<EnemyHealth>();
        if (health != null)
        {
            health.TakeDamage(payload.amount, payload.source);
        }

        Destroy(gameObject);
    }
}