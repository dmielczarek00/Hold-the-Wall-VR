using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RadialMenuItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Button button;
    public Image icon;
    public TMP_Text label;
    public TMP_Text costText;

    [Header("Wygl¹d")]
    public Color hoverColor = new Color(0.5f, 0.8f, 1f, 1f);
    public Color unaffordableColor = new Color(1f, 1f, 1f, 0.5f);

    private TowerData _data;
    private System.Action<TowerData> _onPick;
    private System.Action _onCancel;

    private bool _isCancel;
    private bool _isHovered;
    public bool IsHovered => _isHovered;

    private RadialMenu _owner;

    private string _customLabel;
    private Sprite _customIcon;
    private System.Action _onCustomClick;
    private System.Action _onCustomHover;

    public void InitOwner(RadialMenu owner) => _owner = owner;

    void Awake()
    {
        if (button == null) button = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHovered = true;

        if (_onCustomHover != null)
        {
            _onCustomHover.Invoke();
            return;
        }

        if (_isCancel) _owner?.ShowCancelInfo(label != null ? label.text : "Cancel");
        else if (_data != null) _owner?.ShowInfo(_data);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovered = false;
        // opcjonalnie: _owner?.ClearInfo();
    }

    public void Set(TowerData data, System.Action<TowerData> onPick)
    {
        _isCancel = false;
        _onCancel = null;

        _data = data;
        _onPick = onPick;

        if (icon) icon.sprite = data.icon;
        if (label) label.text = data.displayName;
        if (costText) costText.text = data.cost + " Gold";

        var colors = button.colors;
        colors.highlightedColor = hoverColor;
        colors.selectedColor = hoverColor;
        button.colors = colors;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => _onPick?.Invoke(_data));

        RefreshAffordable();
    }

    public void SetAsCancel(string labelText, Sprite iconSprite, System.Action onCancel)
    {
        _isCancel = true;
        _data = null;
        _onPick = null;
        _onCancel = onCancel;

        if (icon) icon.sprite = iconSprite;
        if (label) label.text = labelText;
        if (costText) costText.text = "";

        var colors = button.colors;
        colors.highlightedColor = hoverColor;
        colors.selectedColor = hoverColor;
        colors.normalColor = Color.white;
        button.colors = colors;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => _onCancel?.Invoke());
        button.interactable = true;
    }

    public void SetAsCustom(string labelText, Sprite iconSprite, System.Action onClick, System.Action onHover = null, TowerData towerData = null)
    {
        _isCancel = false;
        _data = null;
        _onPick = null;

        _onCustomClick = onClick;
        _onCustomHover = onHover;

        if (icon) icon.sprite = iconSprite;
        if (label) label.text = labelText;

        var colors = button.colors;

        if (labelText == "Upgrade")
        {
            if (towerData == null || towerData.upgradeTo == null)
            {
                if (costText) costText.text = "MAX";
                button.interactable = false;

                colors.normalColor = unaffordableColor;
                colors.highlightedColor = unaffordableColor;
                colors.selectedColor = unaffordableColor;
            }
            else
            {
                if (costText) costText.text = towerData.upgradeCost.ToString();

                bool canAfford = GameEconomy.I != null && GameEconomy.I.CanAfford(towerData.upgradeCost);
                button.interactable = canAfford;

                colors.normalColor = canAfford ? Color.white : unaffordableColor;
                colors.highlightedColor = hoverColor;
                colors.selectedColor = hoverColor;
            }
        }
        else
        {
            if (costText) costText.text = "";
            button.interactable = true;

            colors.highlightedColor = hoverColor;
            colors.selectedColor = hoverColor;
        }

        button.colors = colors;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => _onCustomClick?.Invoke());
    }

    void Update()
    {
        RefreshAffordable();
    }

    void RefreshAffordable()
    {
        if (button == null) return;

        if (_isCancel || _data == null)
        {
            button.interactable = true;
            return;
        }

        bool can = GameEconomy.I != null && GameEconomy.I.CanAfford(_data.cost);
        var colors = button.colors;
        colors.normalColor = can ? Color.white : unaffordableColor;
        button.colors = colors;

        button.interactable = can;
    }
}
