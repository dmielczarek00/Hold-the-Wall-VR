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

    public bool IsSpawning => spawning;

    public bool IsDone
    {
        get
        {
            return !spawning && (waves == null || currentWave >= waves.Length);
        }
    }

    void Start()
    {
        if (autoStart && waves != null && waves.Length > 0)
            StartCoroutine(SpawnWave(waves[currentWave]));
    }

    public void RestartFromWave(int startWaveIndex)
    {
        StopAllCoroutines();
        spawning = false;

        if (waves == null || waves.Length == 0)
        {
            currentWave = 0;
            return;
        }

        currentWave = Mathf.Clamp(startWaveIndex, 0, waves.Length);

        if (currentWave < waves.Length)
            StartCoroutine(SpawnWave(waves[currentWave]));
    }

    public void SetWavesAndRestart(Wave[] newWaves, int startWaveIndex)
    {
        waves = newWaves;
        RestartFromWave(startWaveIndex);
    }

    private IEnumerator SpawnWave(Wave wave)
    {
        spawning = true;

        if (wave != null && wave.groups != null)
        {
            foreach (var group in wave.groups)
            {
                if (group == null || group.enemyPrefab == null) continue;

                for (int i = 0; i < group.count; i++)
                {
                    SpawnEnemy(group.enemyPrefab);
                    yield return new WaitForSeconds(group.interval);
                }
            }
        }

        yield return new WaitForSeconds(wave != null ? wave.delayAfterWave : 0f);

        spawning = false;
        currentWave++;

        if (waves != null && currentWave < waves.Length)
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

        var endless = GetComponent<EndlessWaveDirector>();
        if (endless != null)
        {
            var hp = e.GetComponent<EnemyHealth>();
            if (hp != null)
                hp.ApplyMultipliers(endless.HpMultiplier, endless.ArmorMultiplier, endless.GoldMultiplier);
        }
    }
}