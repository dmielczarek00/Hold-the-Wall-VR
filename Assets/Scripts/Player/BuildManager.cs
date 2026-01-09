using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildManager : MonoBehaviour
{
    public static BuildManager I;

    [Header("UI")]
    public RadialMenu radialMenuPrefab;

    void Awake() { I = this; }

    private bool IsPlatformPreviewActive()
    {
        var controller = FindObjectOfType<PlatformPlacementController>();
        return controller != null && controller.IsPlacing;
    }

    public void OpenBuildMenu(BuildSpot spot, Vector3 worldPos)
    {
        if (IsPlatformPreviewActive()) return;
        if (spot == null) return;

        var menu = Instantiate(radialMenuPrefab, worldPos, Quaternion.identity);
        menu.OpenFor(spot);
    }

    public void OpenTowerMenu(BuildSpot spot, Vector3 worldPos)
    {
        if (IsPlatformPreviewActive()) return;
        if (spot == null) return;

        var m = Instantiate(radialMenuPrefab, worldPos, Quaternion.identity);

        TowerData current = null;
        if (spot.CurrentTower != null)
        {
            var tw = spot.CurrentTower.GetComponent<Tower>();
            if (tw != null) current = tw.data;
        }

        m.OpenActionsFor(spot, current);
    }
    public TowerData GetUpgradeTarget(BuildSpot spot)
    {
        if (spot == null || !spot.HasTower) return null;
        var tw = spot.CurrentTower.GetComponent<Tower>();
        if (tw == null || tw.data == null) return null;
        return tw.data.upgradeTo;
    }
    public int GetSellAmount(BuildSpot spot)
    {
        if (spot == null || !spot.HasTower) return 0;
        var tw = spot.CurrentTower.GetComponent<Tower>();
        if (tw == null || tw.data == null) return 0;
        return Mathf.RoundToInt(tw.data.cost * tw.data.sellPercent);
    }
    public bool TryUpgrade(BuildSpot spot)
    {
        if (spot == null || !spot.HasTower) return false;

        var tw = spot.CurrentTower.GetComponent<Tower>();
        if (tw == null || tw.data == null) return false;

        var current = tw.data;
        var next = current.upgradeTo;
        if (next == null) return false;

        int price = current.upgradeCost;
        if (!GameEconomy.I.TrySpend(price)) return false;

        Vector3 pos = spot.buildPoint.position;
        Quaternion rot = spot.buildPoint.rotation;

        Destroy(spot.CurrentTower);
        var newTower = Instantiate(next.prefab, pos, rot);
        spot.Occupy(newTower);

        var newTw = newTower.GetComponent<Tower>();
        if (newTw != null) newTw.data = next;

        return true;
    }

    public bool TrySell(BuildSpot spot)
    {
        if (spot == null || !spot.HasTower) return false;

        var tw = spot.CurrentTower.GetComponent<Tower>();
        if (tw == null || tw.data == null) return false;

        int amt = Mathf.RoundToInt(tw.data.cost * tw.data.sellPercent);
        GameEconomy.I.Add(amt);

        Destroy(spot.CurrentTower);
        spot.Free();

        return true;
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