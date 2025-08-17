using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildManager : MonoBehaviour
{
    public static BuildManager I;

    [Header("UI")]
    public RadialMenu radialMenuPrefab;

    void Awake() { I = this; }

    public void OpenBuildMenu(BuildSpot spot, Vector3 worldPos)
    {
        if (spot == null) return;
        RadialMenu.CloseAll();
        var menu = Instantiate(radialMenuPrefab, worldPos, Quaternion.identity);
        menu.OpenFor(spot);
    }

    public bool TryBuild(BuildSpot spot, TowerData data)
    {
        if (spot == null || data == null) return false;
        if (!spot.IsFree) return false;
        if (!GameEconomy.I.TrySpend(data.cost)) return false;

        var go = Instantiate(data.prefab, spot.buildPoint.position, spot.buildPoint.rotation);
        spot.Occupy(go);
        return true;
    }

    public void Sell(BuildSpot spot, int refund)
    {
        if (spot == null || !spot.HasTower) return;
        Destroy(spot.CurrentTower);
        spot.Free();
        GameEconomy.I.Add(refund);
    }
}