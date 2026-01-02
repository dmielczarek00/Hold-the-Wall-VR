using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Benchmark : MonoBehaviour
{
    public float sampleInterval = 5f;
    public float testDuration = 300f;

    private float elapsed;
    private string filePath;

    void Start()
    {
        string time = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        filePath = Path.Combine(
            Application.persistentDataPath,
            $"benchmark_{time}.csv"
        );

        File.WriteAllText(filePath, "Time;FPS;FrameTimeMs;EnemyCount\n");
        StartCoroutine(LogRoutine());
    }

    IEnumerator LogRoutine()
    {
        while (elapsed < testDuration)
        {
            yield return new WaitForSeconds(sampleInterval);
            elapsed += sampleInterval;

            int enemies = FindObjectsOfType<EnemyHealth>().Length;
            float frameTimeMs = Time.deltaTime * 1000f;
            float fps = 1f / Time.deltaTime;

            File.AppendAllText(
                filePath,
                $"{elapsed:F1};{fps:F1};{frameTimeMs:F2};{enemies}\n"
            );
        }

        Quit();
    }

    void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
