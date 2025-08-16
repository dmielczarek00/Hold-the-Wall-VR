using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public WaypointPath path;         // œcie¿ka
    public float speed = 2f;          // prêdkoœæ chodzenia
    public float reachDistance = 0.2f; // jak blisko punktu musi byæ, ¿eby przejœæ dalej

    private int currentIndex = 0;

    void Update()
    {
        if (path == null || path.points.Length == 0) return;

        Transform targetPoint = path.points[currentIndex];
        Vector3 dir = (targetPoint.position - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;

        // obrót w stronê ruchu
        if (dir.magnitude > 0.01f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 5);

        // sprawdŸ czy dotar³ do punktu
        if (Vector3.Distance(transform.position, targetPoint.position) < reachDistance)
        {
            currentIndex++;
            if (currentIndex >= path.points.Length)
            {
                // dotar³ do koñca
                ReachedGoal();
            }
        }
    }

    void ReachedGoal()
    {
        Debug.Log("Wróg dotar³ do koñca trasy!");
        Destroy(gameObject); // usuñ przeciwnika
    }
}