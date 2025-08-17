using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RadialMenuItem : MonoBehaviour
{
    public Button button;
    public Image icon;
    public TMP_Text label;
    public TMP_Text costText;

    private TowerData _data;
    private System.Action<TowerData> _onPick;

    void Awake()
    {
        if (button == null) button = GetComponent<Button>();
    }

    public void Set(TowerData data, System.Action<TowerData> onPick)
    {
        _data = data;
        _onPick = onPick;

        if (icon) icon.sprite = data.icon;
        if (label) label.text = data.displayName;
        if (costText) costText.text = data.cost.ToString();

        RefreshAffordable();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => _onPick?.Invoke(_data));
    }

    void Update()
    {
        RefreshAffordable();
    }

    void RefreshAffordable()
    {
        bool can = GameEconomy.I != null && GameEconomy.I.CanAfford(_data.cost);
        // jasne gdy staæ, przyciemnienie gdy nie
        var colors = button.colors;
        colors.normalColor = can ? Color.white : new Color(1, 1, 1, 0.5f);
        colors.highlightedColor = can ? Color.white : new Color(1, 1, 1, 0.6f);
        button.colors = colors;
        button.interactable = can; // czy mo¿na kupiæ
    }
}