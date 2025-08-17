using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public WaypointPath path;
    public Transform aimPoint;
    public float speed = 2f;
    public float reachDistance = 0.2f;
    public float enemyHeight = 1.6f;


    private int currentIndex = 0;

    public int CurrentIndex => currentIndex;

    void Update()
    {
        if (path == null || path.points.Length == 0) return;

        Transform targetPoint = path.points[currentIndex];

        // spód przeciwnika
        Vector3 bottom = transform.position + Vector3.down * (enemyHeight * 0.5f);

        // kierunek ruchu
        Vector3 dir = (targetPoint.position - bottom).normalized;
        transform.position += dir * speed * Time.deltaTime;

        // obrót w stronę pełnego kierunku
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5);
        }

        // sprawdzanie czy dotarł
        if (Vector3.Distance(bottom, targetPoint.position) < reachDistance)
        {
            currentIndex++;
            if (currentIndex >= path.points.Length)
            {
                ReachedGoal();
            }
        }
    }

    void ReachedGoal()
    {
        Destroy(gameObject);
    }
}
