using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RadialMenu : MonoBehaviour
{
    [Header("Konfiguracja")]
    public Canvas canvas;
    public RectTransform root;
    public RadialMenuItem itemPrefab;       // przycisk w menu
    public List<TowerData> options = new();
    public float radius = 80f;

    private BuildSpot _spot;

    void Awake()
    {
        if (canvas == null)
            canvas = GetComponentInChildren<Canvas>(true);

        if (canvas != null && canvas.worldCamera == null && Camera.main != null)
            canvas.worldCamera = Camera.main;
    }


    void Update()
    {
        if (Camera.main)
        {
            Vector3 toCam = Camera.main.transform.position - transform.position;
            toCam.y = 0f;
            if (toCam.sqrMagnitude > 1e-4f)
            {
                transform.rotation = Quaternion.LookRotation(-toCam, Vector3.up);
            }
        }
    }

    public void OpenFor(BuildSpot spot)
    {
        _spot = spot;
        BuildButtons();
    }

    void BuildButtons()
    {
        // wyczyœæ stare
        foreach (Transform t in root) Destroy(t.gameObject);

        int n = options.Count;
        for (int i = 0; i < n; i++)
        {
            var item = Instantiate(itemPrefab, root);
            float angle = (360f / n) * i;
            Vector2 pos = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            ) * radius;
            var rt = (RectTransform)item.transform;
            rt.anchoredPosition = pos;

            var data = options[i];
            item.Set(data, OnPick);
        }
    }

    void OnPick(TowerData data)
    {
        if (_spot == null) return;

        bool ok = BuildManager.I.TryBuild(_spot, data);
        if (!ok)
        {
            // Debug.Log("Nie staæ.");
            return;
        }
        Destroy(gameObject);
    }

    public static void CloseAll()
    {
        foreach (var m in FindObjectsOfType<RadialMenu>())
            Destroy(m.gameObject);
    }

    public void Close() => Destroy(gameObject);
}