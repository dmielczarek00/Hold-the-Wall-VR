using UnityEngine;

public class EnemyLadder : MonoBehaviour
{
    [Header("Punkty wejścia i wyjścia")]
    public Transform bottomPoint;    // wejście
    public Transform topPoint;       // wyjście

    [Header("Parametry wspinaczki")]
    public float climbDuration = 2f; // czas wejścia po drabinie


    private void OnDrawGizmos()
    {
        if (bottomPoint && topPoint)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(bottomPoint.position, topPoint.position);
            Gizmos.DrawSphere(bottomPoint.position, 0.1f);
            Gizmos.DrawSphere(topPoint.position, 0.1f);
        }
    }
}