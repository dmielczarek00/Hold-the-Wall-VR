using UnityEngine;
using UnityEngine.InputSystem;

public class ToggleWeaponMode : MonoBehaviour
{
    [Header("Input (New Input System)")]
    public InputActionReference toggleAction;

    [Header("Obiekty do w³¹czenia przy trybie 'miecz'")]
    public GameObject[] enableOnWeapon;

    [Header("Obiekty do wy³¹czenia przy trybie 'miecz'")]
    public GameObject[] disableOnWeapon;

    [Header("Skrypty do wy³¹czenia przy trybie 'miecz'")]
    public MonoBehaviour[] disableScriptsOnWeapon;

    private bool weaponActive;

    private void OnEnable()
    {
        if (toggleAction != null)
            toggleAction.action.performed += OnToggle;
    }

    private void OnDisable()
    {
        if (toggleAction != null)
            toggleAction.action.performed -= OnToggle;
    }

    private void OnToggle(InputAction.CallbackContext ctx)
    {
        weaponActive = !weaponActive;

        // W³¹cz/wy³¹cz obiekty
        foreach (var obj in enableOnWeapon)
            if (obj != null) obj.SetActive(weaponActive);

        foreach (var obj in disableOnWeapon)
            if (obj != null) obj.SetActive(!weaponActive);

        // W³¹cz/wy³¹cz skrypty
        foreach (var script in disableScriptsOnWeapon)
            if (script != null) script.enabled = !weaponActive;
    }
}