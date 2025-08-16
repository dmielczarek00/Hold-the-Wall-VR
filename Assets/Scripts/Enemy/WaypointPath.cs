using UnityEngine;

public class WaypointPath : MonoBehaviour
{
    [Header("Ścieżka")]
    public Transform[] points;

    [Header("Podgląd gizmo")]
    public float enemyRadius = 0.3f;
    public float enemyHeight = 1.6f;
    public float pathHeightOffset = 0.0f;

    void OnDrawGizmos()
    {
        if (points == null || points.Length < 2) return;

        Gizmos.color = Color.green;

        for (int i = 0; i < points.Length; i++)
        {
            if (points[i] == null) continue;

            Vector3 pos = points[i].position + Vector3.up * pathHeightOffset;

            // Rysuj pionowy cylinder symbolizujący przeciwnika
            DrawEnemyGizmo(pos);

            // Połącz punkty linią
            if (i < points.Length - 1 && points[i + 1] != null)
            {
                Vector3 nextPos = points[i + 1].position + Vector3.up * pathHeightOffset;
                Gizmos.DrawLine(pos, nextPos);
            }
        }
    }

    private void DrawEnemyGizmo(Vector3 pos)
    {
        // cylinder narysowany jako 2 dyski + linie pionowe
        int segments = 16;
        float step = Mathf.PI * 2f / segments;

        Vector3 prev = pos + new Vector3(Mathf.Cos(0) * enemyRadius, 0, Mathf.Sin(0) * enemyRadius);
        Vector3 prevTop = prev + Vector3.up * enemyHeight;

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * step;
            Vector3 next = pos + new Vector3(Mathf.Cos(angle) * enemyRadius, 0, Mathf.Sin(angle) * enemyRadius);
            Vector3 nextTop = next + Vector3.up * enemyHeight;

            // podstawa
            Gizmos.DrawLine(prev, next);
            // góra
            Gizmos.DrawLine(prevTop, nextTop);
            // pionowe łączenia
            Gizmos.DrawLine(prev, prevTop);

            prev = next;
            prevTop = nextTop;
        }
    }
}
