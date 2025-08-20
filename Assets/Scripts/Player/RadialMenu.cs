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

    [Header("Układ przycisków")]
    public float spacing = 16f;
    public float itemScale = 1f;
    public float angleOffset = -90f;        // pozycja pierwszego
    public float menuDistance = 1.5f;       // odstęp menu od wieżyczki
    public float menuHeightOffset = 1.8f;   // podniesienie menu
    public float minButtonRadius = 120f;    // minimalny odstęp przycisków od środka

    [Tooltip("Przybliżenie gdy gracz jest daleko")]
    public float distanceFactor = 0.7f;
    public float minMenuDistance = 1.5f;
    public float distanceThreshold = 5f;

    [Header("Panel info")]
    public GameObject infoRoot;
    public TMP_Text infoTitle;
    public TMP_Text infoDesc;
    public TMP_Text infoStats;

    [Header("Ikony dla wieży")]
    public Sprite upgradeIcon;
    public string upgradeLabel = "Upgrade";
    public Sprite sellIcon;
    public string sellLabel = "Sell";
    public Sprite cancelIcon;
    public string cancelLabel = "Cancel";

    private BuildSpot _spot;

    private float _hoverTimer = 0f;
    public float hoverTimeout = 5f;

    private TowerData _currentTowerData;

    void Awake()
    {
        if (canvas == null)
            canvas = GetComponentInChildren<Canvas>(true);

        if (canvas != null && canvas.worldCamera == null && Camera.main != null)
            canvas.worldCamera = Camera.main;
    }

    void Update()
    {
        if (Camera.main && _spot != null)
        {
            UpdateMenuTransform();
        }

        if (IsAnyItemHovered())
        {
            _hoverTimer = 0f;
        }
        else
        {
            _hoverTimer += Time.deltaTime;
            if (_hoverTimer >= hoverTimeout)
            {
                Close();
            }
        }
    }

    bool IsAnyItemHovered()
    {
        foreach (Transform t in root)
        {
            var item = t.GetComponent<RadialMenuItem>();
            if (item != null && item.IsHovered) return true;
        }
        return false;
    }

    public void OpenFor(BuildSpot spot)
    {
        _spot = spot;
        _currentTowerData = null;
        BuildButtons_Build();
        if (infoRoot) infoRoot.SetActive(false);

        UpdateMenuTransform();
    }
    void UpdateMenuTransform()
    {
        if (Camera.main == null || _spot == null) return;

        var cam = Camera.main;
        Vector3 spotPos = _spot.transform.position;

        Vector3 toCam = cam.transform.position - spotPos;
        if (toCam.sqrMagnitude < 1e-6f) return;

        Vector3 dir = toCam.normalized;
        float camDist = toCam.magnitude;

        float offset = menuDistance;
        if (camDist > distanceThreshold)
            offset += (camDist - distanceThreshold) * distanceFactor;

        offset = Mathf.Max(offset, minMenuDistance);

        // Pozycja menu
        transform.position = spotPos + Vector3.up * menuHeightOffset + dir * offset;

        // Rotacja menu
        Vector3 lookDir = (transform.position - cam.transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
    }

    public void OpenActionsFor(BuildSpot spot, TowerData current)
    {
        _spot = spot;
        _currentTowerData = current; // tryb akcji
        BuildButtons_Actions();
        if (infoRoot) infoRoot.SetActive(false);

        UpdateMenuTransform();
    }

    public void ShowInfo(TowerData d)
    {
        if (d == null) { ClearInfo(); return; }

        if (infoRoot) infoRoot.SetActive(true);
        if (infoTitle) infoTitle.text = d.displayName;
        if (infoDesc) infoDesc.text = d.description;

        TowerData current = _spot?.CurrentTower?.GetComponent<Tower>()?.data;

        string StatLine(string label, float newVal, float? oldVal, string format = "0.##")
        {
            string diffText = "";
            if (oldVal.HasValue)
            {
                float diff = newVal - oldVal.Value;
                if (Mathf.Abs(diff) > 1e-4f)
                {
                    string color = diff > 0 ? "#00FF00" : "#FF0000";
                    string sign = diff > 0 ? "+" : "";
                    diffText = $" <color={color}>({sign}{diff.ToString(format)})</color>";
                }
            }
            return $"{label}: {newVal.ToString(format)}{diffText}";
        }

        if (infoStats)
        {
            infoStats.text =
                StatLine("Range", d.range, current?.range) + "\n" +
                StatLine("Fire rate", d.fireRate, current?.fireRate) + "/s\n" +
                StatLine("Accuracy", d.accuracy * 100f, current != null ? current.accuracy * 100f : (float?)null, "0") + "%\n" +
                StatLine("Damage", d.damage, current?.damage, "0") + "\n" +
                StatLine("Armor Penetration.", d.armorPenetration, current?.armorPenetration, "0") + "\n" +
                StatLine("Shred", d.shred, current?.shred, "0") + "\n" +
                StatLine("Projectile Speed", d.projectileSpeed, current?.projectileSpeed);
        }
    }

    public void ShowCancelInfo(string label)
    {
        if (infoRoot) infoRoot.SetActive(true);
        if (infoTitle) infoTitle.text = label;
        if (infoDesc) infoDesc.text = "";
        if (infoStats) infoStats.text = "";
    }
    public void ShowSellInfo(int sellAmount)
    {
        if (infoRoot) infoRoot.SetActive(true);
        if (infoTitle) infoTitle.text = sellLabel;
        if (infoDesc) infoDesc.text = $"You will get {sellAmount} $";
        if (infoStats) infoStats.text = "";
    }
    public void ClearInfo()
    {
        if (infoTitle) infoTitle.text = "";
        if (infoDesc) infoDesc.text = "";
        if (infoStats) infoStats.text = "";
        if (infoRoot) infoRoot.SetActive(false);
    }

    void BuildButtons_Build()
    {
        foreach (Transform t in root) Destroy(t.gameObject);

        int n = options.Count + 1;
        if (n <= 0) return;

        var prefabRT = itemPrefab.GetComponent<RectTransform>();
        float itemW = prefabRT.sizeDelta.x;
        float itemH = prefabRT.sizeDelta.y;
        float itemDia = Mathf.Max(itemW, itemH) * Mathf.Max(0.0001f, 1f);

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
            Vector2 pos = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            ) * R;
            rt.anchoredPosition = pos;

            if (i == 0)
            {
                item.SetAsCancel(cancelLabel, cancelIcon, Close);
            }
            else
            {
                var data = options[i - 1];
                item.Set(data, OnPick);
            }
        }
    }
    void BuildButtons_Actions()
    {
        foreach (Transform t in root) Destroy(t.gameObject);

        int n = 3;

        var prefabRT = itemPrefab.GetComponent<RectTransform>();
        float itemW = prefabRT.sizeDelta.x;
        float itemH = prefabRT.sizeDelta.y;
        float itemDia = Mathf.Max(itemW, itemH) * Mathf.Max(0.0001f, 1f);

        float chord = itemDia + Mathf.Max(0f, spacing);
        float R = chord / (2f * Mathf.Sin(Mathf.PI / n));
        if (R < minButtonRadius) R = minButtonRadius;

        for (int i = 0; i < n; i++)
        {
            var item = Instantiate(itemPrefab, root);
            item.InitOwner(this);

            var rt = (RectTransform)item.transform;
            rt.localScale = Vector3.one * itemScale;

            float angle = (360f / n) * i + angleOffset;
            Vector2 pos = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            ) * R;
            rt.anchoredPosition = pos;

            if (i == 0)
            {
                // Cancel
                item.SetAsCancel(cancelLabel, cancelIcon, Close);
            }
            else if (i == 1)
            {
                // Upgrade
                item.SetAsCustom(
                    upgradeLabel,
                    upgradeIcon,
                    onClick: () =>
                    {
                        if (_spot) BuildManager.I.TryUpgrade(_spot);
                        Close();
                    },
                    onHover: () =>
                    {
                        var current = _spot?.CurrentTower?.GetComponent<Tower>()?.data;
                        var next = BuildManager.I.GetUpgradeTarget(_spot);

                        if (next != null && current != null)
                        {
                            ShowInfo(next);
                            if (infoDesc)
                                infoDesc.text = $"Upgrade cost: {current.upgradeCost}";
                        }
                        else
                        {
                            ShowCancelInfo(upgradeLabel);
                        }
                    },
                    towerData: _currentTowerData
                );
            }
            else
            {
                // Sell
                item.SetAsCustom(
                    sellLabel,
                    sellIcon,
                    onClick: () =>
                    {
                        if (_spot) BuildManager.I.TrySell(_spot);
                        Close();
                    },
                    onHover: () =>
                    {
                        int amt = BuildManager.I.GetSellAmount(_spot);
                        ShowSellInfo(amt);
                    }
                );
            }
        }
    }
    void OnPick(TowerData data)
    {
        if (_spot == null) return;

        bool ok = BuildManager.I.TryBuild(_spot, data);
        if (!ok)
        {
            // Debug.Log("Nie stać.");
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
