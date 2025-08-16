using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public WaypointPath path;
    public float spawnInterval = 3f;
    public int maxEnemies = 10;

    int spawned = 0;
    float timer = 0;

    void Update()
    {
        if (spawned >= maxEnemies) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0;
            var e = Instantiate(enemyPrefab, transform.position, Quaternion.identity);

            var mover = e.GetComponent<EnemyMovement>();
            if (mover != null)
            {
                mover.path = path;
            }
            else
            {
                Debug.LogError("Prefab enemy nie ma EnemyMovement!");
            }

            spawned++;
        }
    }
}
