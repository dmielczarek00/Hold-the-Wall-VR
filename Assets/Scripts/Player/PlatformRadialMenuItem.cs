using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlatformRadialMenuItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Button button;
    public Image icon;
    public TMP_Text label;
    public TMP_Text costText;

    [Header("Wygl¹d")]
    public Color hoverColor = new Color(0.5f, 0.8f, 1f, 1f);
    public Color unaffordableColor = new Color(1f, 1f, 1f, 0.5f);

    private PlatformData _data;
    private System.Action<PlatformData> _onPick;
    private System.Action _onCancel;

    private bool _isCancel;
    private bool _isHovered;
    public bool IsHovered => _isHovered;

    private PlatformRadialMenu _owner;

    private Color _normalColor;
    private bool _colorsCached;

    void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (button != null && !_colorsCached)
        {
            _normalColor = button.colors.normalColor;
            _colorsCached = true;
        }

        if (button != null)
            button.onClick.AddListener(OnClick);
    }

    public void InitOwner(PlatformRadialMenu owner)
    {
        _owner = owner;
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

        if (button != null)
        {
            var colors = button.colors;
            colors.normalColor = _normalColor;
            colors.highlightedColor = hoverColor;
            button.colors = colors;
            button.interactable = true;
        }
    }

    public void Set(PlatformData data, System.Action<PlatformData> onPick)
    {
        _isCancel = false;
        _onCancel = null;

        _data = data;
        _onPick = onPick;

        if (icon) icon.sprite = data.icon;
        if (label) label.text = data.displayName;
        if (costText) costText.text = data.cost + " Gold";

        if (button != null)
        {
            var colors = button.colors;
            colors.highlightedColor = hoverColor;
            button.colors = colors;
        }

        UpdateAffordableState();
    }

    void OnClick()
    {
        if (_isCancel)
        {
            _onCancel?.Invoke();
            return;
        }

        if (_data != null)
        {
            _onPick?.Invoke(_data);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHovered = true;
        if (button == null) return;

        var colors = button.colors;
        colors.normalColor = hoverColor;
        button.colors = colors;

        if (!_isCancel && _owner != null && _data != null)
        {
            _owner.ShowInfo(_data);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovered = false;
        if (button == null){
            if (_owner != null)
            {
                _owner.HideInfo();
            }
            return;
        }

        var colors = button.colors;
        colors.normalColor = _normalColor;
        button.colors = colors;
        UpdateAffordableState();
    }

    void UpdateAffordableState()
    {
        if (button == null) return;

        if (_isCancel || _data == null)
        {
            button.interactable = true;
            return;
        }

        bool can = GameEconomy.I != null && GameEconomy.I.CanAfford(_data.cost);
        var colors = button.colors;
        colors.normalColor = can ? _normalColor : unaffordableColor;
        button.colors = colors;

        button.interactable = can;
    }
}