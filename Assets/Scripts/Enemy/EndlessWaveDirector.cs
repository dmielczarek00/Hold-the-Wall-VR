using UnityEngine;

public class EndlessWaveDirector : MonoBehaviour
{
    public EnemySpawner spawner;

    public int skipWavesAfterFirst = 2;

    public float countMultiplierStep = 1.5f;
    public float hpMultiplierStep = 1.2f;
    public float armorMultiplierStep = 1.2f;
    public float goldMultiplierStep = 1.1f;

    public float HpMultiplier { get; private set; } = 1f;
    public float ArmorMultiplier { get; private set; } = 1f;
    public float GoldMultiplier { get; private set; } = 1f;

    private float _countMultiplier = 1f;
    private Wave[] _baseWaves;
    private bool _firstRun = true;

    void Awake()
    {
        if (spawner == null) spawner = GetComponent<EnemySpawner>();

        _baseWaves = CloneWaves(spawner != null ? spawner.waves : null);
    }

    void Update()
    {
        if (spawner == null) return;
        if (!spawner.IsDone) return;

        NextCycle();
    }

    void NextCycle()
    {
        _countMultiplier *= countMultiplierStep;
        HpMultiplier *= hpMultiplierStep;
        ArmorMultiplier *= armorMultiplierStep;
        GoldMultiplier *= goldMultiplierStep;

        var scaled = BuildScaledWaves(_baseWaves, _countMultiplier);

        int startIndex = 0;
        if (!_firstRun) startIndex = Mathf.Max(0, skipWavesAfterFirst);

        spawner.SetWavesAndRestart(scaled, startIndex);

        _firstRun = false;
    }

    Wave[] BuildScaledWaves(Wave[] src, float countMul)
    {
        if (src == null) return null;

        var waves = new Wave[src.Length];
        for (int w = 0; w < src.Length; w++)
        {
            var sWave = src[w];
            var nWave = new Wave();
            nWave.name = sWave.name;

            nWave.delayAfterWave = Random.Range(0f, 15f);

            if (sWave.groups == null)
            {
                nWave.groups = new EnemyGroup[0];
                waves[w] = nWave;
                continue;
            }

            nWave.groups = new EnemyGroup[sWave.groups.Length];
            for (int g = 0; g < sWave.groups.Length; g++)
            {
                var sg = sWave.groups[g];
                var ng = new EnemyGroup();
                ng.enemyPrefab = sg.enemyPrefab;

                ng.interval = Random.Range(1f, 3f);

                ng.count = Mathf.Max(1, Mathf.RoundToInt(sg.count * countMul));
                nWave.groups[g] = ng;
            }

            waves[w] = nWave;
        }

        return waves;
    }

    Wave[] CloneWaves(Wave[] src)
    {
        if (src == null) return null;

        var waves = new Wave[src.Length];
        for (int w = 0; w < src.Length; w++)
        {
            var sWave = src[w];
            var nWave = new Wave();
            nWave.name = sWave.name;
            nWave.delayAfterWave = sWave.delayAfterWave;

            if (sWave.groups == null)
            {
                nWave.groups = new EnemyGroup[0];
                waves[w] = nWave;
                continue;
            }

            nWave.groups = new EnemyGroup[sWave.groups.Length];
            for (int g = 0; g < sWave.groups.Length; g++)
            {
                var sg = sWave.groups[g];
                var ng = new EnemyGroup();
                ng.enemyPrefab = sg.enemyPrefab;
                ng.count = sg.count;
                ng.interval = sg.interval;
                nWave.groups[g] = ng;
            }

            waves[w] = nWave;
        }

        return waves;
    }
}