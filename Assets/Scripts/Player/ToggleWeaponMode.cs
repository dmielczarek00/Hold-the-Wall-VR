using UnityEngine;

public class ToggleWeaponMode : MonoBehaviour
{
    public enum WeaponMode
    {
        Build,
        Sword,
        Crossbow
    }

    [Header("Obiekty do w³¹czenia przy trybie miecza")]
    public GameObject[] enableOnWeapon;

    [Header("Obiekty do wy³¹czenia w trybie miecza lub kuszy")]
    public GameObject[] disableOnWeapon;

    [Header("Skrypty do wy³¹czenia w trybie miecza lub kuszy")]
    public MonoBehaviour[] disableScriptsOnWeapon;

    [Header("Obiekty do w³¹czenia w trybie kuszy")]
    public GameObject[] enableOnCrossbow;

    [Header("DŸwiêki")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip swordEquipSound;
    [SerializeField] private AudioClip crossbowEquipSound;

    private WeaponMode currentMode = WeaponMode.Build;
    public WeaponMode CurrentMode => currentMode;
    public bool IsBuildMode => currentMode == WeaponMode.Build;

    public void SetBuildMode()
    {
        SetWeaponState(WeaponMode.Build);
    }
    public void SetWeaponState(WeaponMode mode)
    {
        if (mode == currentMode)
            return;

        if (mode == WeaponMode.Sword)
            AudioPlay.PlaySound(audioSource, swordEquipSound);
        else if (mode == WeaponMode.Crossbow)
            AudioPlay.PlaySound(audioSource, crossbowEquipSound);

        currentMode = mode;

        bool build = mode == WeaponMode.Build;
        bool sword = mode == WeaponMode.Sword;
        bool crossbow = mode == WeaponMode.Crossbow;
        bool weaponActive = sword || crossbow;

        // Sword visuals
        foreach (var obj in enableOnWeapon)
            if (obj != null) obj.SetActive(sword);

        // Crossbow visuals
        foreach (var obj in enableOnCrossbow)
            if (obj != null) obj.SetActive(crossbow);

        // Rzeczy wspólne
        foreach (var obj in disableOnWeapon)
            if (obj != null) obj.SetActive(!weaponActive);

        foreach (var script in disableScriptsOnWeapon)
            if (script != null) script.enabled = !weaponActive;
    }
}