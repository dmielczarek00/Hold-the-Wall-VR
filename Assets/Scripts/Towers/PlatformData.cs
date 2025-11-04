using UnityEngine;

[CreateAssetMenu(menuName = "TD/Platform", fileName = "PlatformData")]
public class PlatformData : ScriptableObject
{
    [Header("Meta")]
    public string displayName;
    public Sprite icon;
    public GameObject prefab;
    [TextArea(2, 4)] public string description;

    [Header("Ekonomia")]
    public int cost = 50;

    [Header("Strefa stawiania")]
    public string allowedTag = "BuildArea";

    [Header("Prefab podgl¹du")]
    [Tooltip("Jeœli puste, do podgl¹du bêdzie u¿yty zwyk³y prefab.")]
    public GameObject ghostPrefab;

    [Header("Rozmiar na p³aszczyŸnie XZ")]
    [Tooltip("Promieñ zajmowany przez platformê.")]
    public float footprintRadius = 0.5f;
}