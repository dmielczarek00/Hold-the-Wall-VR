using System.Collections.Generic;
using UnityEngine;

public class EndlessWaveDirector : MonoBehaviour
{
    public EnemySpawner[] spawners;
    public int skipWavesAfterFirst = 2;

    public float countMultiplierStep = 1.5f;
    public float hpMultiplierStep = 1.2f;
    public float armorMultiplierStep = 1.2f;
    public float goldMultiplierStep = 1.1f;

    public Vector2 delayAfterWaveRange = new Vector2(0f, 15f);
    public Vector2 intervalRange = new Vector2(1f, 3f);

    class State
    {
        public bool firstRun = true;
        public float countMul = 1f;
        public float hpMul = 1f;
        public float armorMul = 1f;
        public float goldMul = 1f;
        public Wave[] baseWaves;
    }

    private readonly Dictionary<EnemySpawner, State> _state = new();

    void Awake()
    {
        if (spawners == null || spawners.Length == 0)
            spawners = FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None);

        _state.Clear();
        foreach (var s in spawners)
        {
            if (s == null) continue;
            _state[s] = new State { baseWaves = CloneWaves(s.waves) };
        }
    }

    void Update()
    {
        if (spawners == null) return;

        for (int i = 0; i < spawners.Length; i++)
        {
            var s = spawners[i];
            if (s == null) continue;

            // Ka¿dy spawner dzia³a niezale¿nie
            if (s.IsDone)
                NextCycleFor(s);
        }
    }

    void NextCycleFor(EnemySpawner spawner)
    {
        if (!_state.TryGetValue(spawner, out var st) || st == null)
        {
            st = new State { baseWaves = CloneWaves(spawner.waves) };
            _state[spawner] = st;
        }
            st.countMul *= countMultiplierStep;
            st.hpMul *= hpMultiplierStep;
            st.armorMul *= armorMultiplierStep;
            st.goldMul *= goldMultiplierStep;

        int startIndex = st.firstRun ? 0 : Mathf.Max(0, skipWavesAfterFirst);

        var scaled = BuildScaledWaves(st.baseWaves, st.countMul, st.hpMul, st.armorMul, st.goldMul);
        spawner.SetWavesAndRestart(scaled, startIndex);

        st.firstRun = false;
    }

    Wave[] BuildScaledWaves(Wave[] src, float countMul, float hpMul, float armorMul, float goldMul)
    {
        if (src == null) return null;

        var waves = new Wave[src.Length];
        for (int w = 0; w < src.Length; w++)
        {
            var sWave = src[w];
            var nWave = new Wave();
            nWave.name = sWave.name;

            nWave.delayAfterWave = Random.Range(delayAfterWaveRange.x, delayAfterWaveRange.y);

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

                ng.interval = Random.Range(intervalRange.x, intervalRange.y);
                ng.count = Mathf.Max(1, Mathf.RoundToInt(sg.count * countMul));

                ng.maxHealth = Mathf.Max(1, Mathf.RoundToInt(sg.maxHealth * hpMul));
                ng.maxArmor = Mathf.Max(0, Mathf.RoundToInt(sg.maxArmor * armorMul));
                ng.moneyReward = Mathf.Max(0, Mathf.RoundToInt(sg.moneyReward * goldMul));

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

                ng.maxHealth = sg.maxHealth;
                ng.maxArmor = sg.maxArmor;
                ng.moneyReward = sg.moneyReward;

                nWave.groups[g] = ng;
            }

            waves[w] = nWave;
        }

        return waves;
    }
}