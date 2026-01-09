using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(XRSimpleInteractable))]
public class BuildSpot : MonoBehaviour
{
    [Tooltip("Miejsce postawienia wie¿y; jeœli puste, bêdzie u¿yty transform tego obiektu.")]
    public Transform buildPoint;

    private GameObject _tower;   // aktualnie postawiona wie¿a

    public bool IsFree => _tower == null;
    public bool HasTower => _tower != null;
    public GameObject CurrentTower => _tower;

    public void Occupy(GameObject tower) => _tower = tower;
    public void Free() => _tower = null;

    private XRSimpleInteractable interactable;

    void Awake()
    {
        if (buildPoint == null) buildPoint = transform;

        interactable = GetComponent<XRSimpleInteractable>();
        interactable.selectEntered.AddListener(OnSelectEntered);
    }

    private void OnDestroy()
    {
        if (interactable != null)
            interactable.selectEntered.RemoveListener(OnSelectEntered);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (!IsBuildModeInteractor(args.interactorObject))
            return;

        if (IsPlatformPreviewActive())
            return;

        OpenMenu();
    }

    private bool IsBuildModeInteractor(object interactorObject)
    {
        if (interactorObject is Component c)
        {
            var toggle = c.GetComponentInParent<ToggleWeaponMode>();
            return toggle != null && toggle.IsBuildMode;
        }

        return false;
    }

    private bool IsPlatformPreviewActive()
    {
        var controller = FindObjectOfType<PlatformPlacementController>();
        return controller != null && controller.IsPlacing;
    }

    public void OpenMenu()
    {
        RadialMenu.CloseAll();

        if (HasTower)
            BuildManager.I.OpenTowerMenu(this, transform.position + Vector3.up * 2f);
        else
            BuildManager.I.OpenBuildMenu(this, transform.position + Vector3.up * 2f);
    }
}
