using System.Collections;
using UnityEngine;

public class DamageVignette : MonoBehaviour
{
    [Header("Referencje")]
    public CanvasGroup vignetteGroup;

    [Header("Parametry efektu")]
    public float maxAlpha = 0.5f;
    public float fadeDuration = 0.3f;

    private Coroutine _currentRoutine;

    private void Awake()
    {
        if (vignetteGroup != null)
        {
            vignetteGroup.alpha = 0f;
        }
    }

    // Wywo³aj przy otrzymaniu obra¿eñ
    public void PlayVignette()
    {
        if (vignetteGroup == null) return;

        if (_currentRoutine != null)
            StopCoroutine(_currentRoutine);

        _currentRoutine = StartCoroutine(VignetteRoutine());
    }

    private IEnumerator VignetteRoutine()
    {
        // start od maxAlpha
        float t = 0f;
        vignetteGroup.alpha = maxAlpha;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float normalized = t / fadeDuration;
            vignetteGroup.alpha = Mathf.Lerp(maxAlpha, 0f, normalized);
            yield return null;
        }

        vignetteGroup.alpha = 0f;
        _currentRoutine = null;
    }
}