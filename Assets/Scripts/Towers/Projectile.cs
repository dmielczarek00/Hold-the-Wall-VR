using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    public float speed = 15f;
    public int damage = 1;
    private Vector3 direction;

    public void SetTarget(Vector3 targetPosition)
    {
        direction = (targetPosition - transform.position).normalized;
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        var enemy = other.GetComponent<EnemyMovement>();
        if (enemy != null)
        {
            Destroy(other.gameObject);
            Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}