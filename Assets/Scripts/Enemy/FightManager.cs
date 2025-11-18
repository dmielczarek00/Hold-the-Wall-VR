using UnityEngine;

public class FightManager : MonoBehaviour
{
    public static FightManager Instance;

    [Header("Arena walki")]
    public Collider arenaCollider;   // collider wyznaczający obszar, w którym mogą poruszać się przeciwnicy

    [Header("Walka wręcz")]
    public int maxSimultaneousAttackers = 4;   // ilu przeciwników może jednocześnie stać przy graczu

    void Awake()
    {
        // singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // przycina pozycję do granic areny
    public Vector3 ClampToArena(Vector3 worldPos)
    {
        if (arenaCollider == null)
            return worldPos;

        Vector3 pos = worldPos + Vector3.up * 0.1f;
        Vector3 closest = arenaCollider.ClosestPoint(pos);

        // poprawka tylko w poziomie, wysokość zostaje
        closest.y = worldPos.y;
        return closest;
    }
}