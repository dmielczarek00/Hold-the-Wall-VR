using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TD/Tower", fileName = "TowerData")]
public class TowerData : ScriptableObject
{
    public string displayName;
    public GameObject prefab;
    public Sprite icon;
    public int cost = 50;
}