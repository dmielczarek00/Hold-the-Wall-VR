using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlatformRadialMenu : MonoBehaviour
{
    [Header("Konfiguracja")]
    public Canvas canvas;
    public RectTransform root;
    public PlatformRadialMenuItem itemPrefab;

    [Header("Dostêpne platformy")]
    public List<PlatformData> platforms = new();

    [Header("Uk³ad przycisków")]
    public float spacing = 16f;
    public float itemScale = 1f;
    public float angleOffset = -90f;
    public float menuDistance = 1.5f;
    public float minButtonRadius = 120f;

    [Header("Opis")]
    public GameObject infoRoot;
    public TMP_Text infoTitle;
    public TMP_Text infoDesc;

    [Header("Ikony")]
    public Sprite cancelIcon;
    public string cancelLabel = "Anuluj";

    private PlatformPlacementController _placementController;

    void Awake()
    {
        if (canvas == null)
            canvas = GetComponentInChildren<Canvas>(true);

        if (canvas != null && canvas.worldCamera == null && Camera.main != null)
            canvas.worldCamera = Camera.main;
    }

    void Update()
    {
        if (Camera.main != null)
            UpdateMenuTransform();
    }

    public void Init(PlatformPlacementController controller)
    {
        _placementController = controller;
        BuildButtons();
        if (infoRoot) infoRoot.SetActive(false);
        UpdateMenuTransform();
    }

    void UpdateMenuTransform()
    {
        var cam = Camera.main;
        if (cam == null) return;

        Vector3 pos = cam.transform.position + cam.transform.forward * menuDistance;
        transform.position = pos;
        transform.rotation = Quaternion.LookRotation((transform.position - cam.transform.position).normalized, Vector3.up);
    }

    void BuildButtons()
    {
        foreach (Transform t in root) Destroy(t.gameObject);

        int n = (platforms != null ? platforms.Count : 0) + 1;
        if (n <= 0 || itemPrefab == null || root == null) return;

        float itemDia = Mathf.Max(itemPrefab.GetComponent<RectTransform>().sizeDelta.x,
                                  itemPrefab.GetComponent<RectTransform>().sizeDelta.y);
        float chord = itemDia + Mathf.Max(0f, spacing);
        float R = (n == 1) ? 0f : chord / (2f * Mathf.Sin(Mathf.PI / n));
        if (R < minButtonRadius) R = minButtonRadius;

        for (int i = 0; i < n; i++)
        {
            var item = Instantiate(itemPrefab, root);
            item.InitOwner(this);
            var rt = (RectTransform)item.transform;
            rt.localScale = Vector3.one * itemScale;

            float angle = (360f / n) * i + angleOffset;
            Vector2 pos = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * R;
            rt.anchoredPosition = pos;

            if (i == 0)
            {
                item.SetAsCancel(cancelLabel, cancelIcon, Close);
            }
            else
            {
                var data = platforms[i - 1];
                item.Set(data, OnPick);
            }
        }
    }

    void OnPick(PlatformData data)
    {
        if (_placementController == null || data == null) return;
        _placementController.BeginPlacement(data);
        Destroy(gameObject);
    }

    public static void CloseAll()
    {
        foreach (var m in FindObjectsOfType<PlatformRadialMenu>())
            Destroy(m.gameObject);
    }

    public void Close()
    {
        Destroy(gameObject);
    }
}