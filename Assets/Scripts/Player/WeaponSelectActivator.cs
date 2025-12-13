using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class WeaponSelectActivator : MonoBehaviour
{
    public ToggleWeaponMode.WeaponMode mode = ToggleWeaponMode.WeaponMode.Build;

    private XRSimpleInteractable interactable;

    private void Awake()
    {
        interactable = GetComponent<XRSimpleInteractable>();
    }

    private void OnEnable()
    {
        interactable.selectEntered.AddListener(OnSelect);
    }

    private void OnDisable()
    {
        interactable.selectEntered.RemoveListener(OnSelect);
    }

    private void OnSelect(SelectEnterEventArgs args)
    {
        var interactorTransform = args.interactorObject.transform;
        var weapon = interactorTransform.GetComponentInParent<ToggleWeaponMode>();

        if (weapon != null)
            weapon.SetWeaponState(mode);
    }
}