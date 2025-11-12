using UnityEngine;
using System.Collections;

[System.Serializable]
public class EnemyGroup
{
    public GameObject enemyPrefab;
    public int count;
    public float interval;
}

[System.Serializable]
public class Wave
{
    public string name;
    public EnemyGroup[] groups;
    public float delayAfterWave = 5f;
}

public class EnemySpawner : MonoBehaviour
{
    public WaypointPath path;
    public Wave[] waves;
    public bool autoStart = true;

    [Header("Drabina")]
    public EnemyLadder ladderGoal;

    private int currentWave = 0;
    private bool spawning = false;

    void Start()
    {
        if (autoStart && waves.Length > 0)
            StartCoroutine(SpawnWave(waves[currentWave]));
    }

    public void StartNextWave()
    {
        if (spawning || currentWave >= waves.Length) return;
        StartCoroutine(SpawnWave(waves[currentWave]));
    }

    private IEnumerator SpawnWave(Wave wave)
    {
        spawning = true;
        Debug.Log("Spawning wave: " + wave.name);

        foreach (var group in wave.groups)
        {
            for (int i = 0; i < group.count; i++)
            {
                SpawnEnemy(group.enemyPrefab);
                yield return new WaitForSeconds(group.interval);
            }
        }

        yield return new WaitForSeconds(wave.delayAfterWave);

        spawning = false;
        currentWave++;

        if (currentWave < waves.Length)
        {
            StartCoroutine(SpawnWave(waves[currentWave]));
        }
    }

    private void SpawnEnemy(GameObject prefab)
    {
        var e = Instantiate(prefab, transform.position, Quaternion.identity);

        var mover = e.GetComponent<EnemyMovement>();
        if (mover != null)
        {
            mover.path = path;
            mover.ladderGoal = ladderGoal;
        }
        else
        {
            Debug.LogError("Prefab enemy nie ma EnemyMovement!");
        }
    }
}