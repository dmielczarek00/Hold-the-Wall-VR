using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealthBar : MonoBehaviour
{
    public EnemyHealth target;

    [Header("Paski")]
    public RectTransform hpBar;
    public RectTransform armorBar;

    [Header("T³o pancerza")]
    public GameObject armorBackground;

    [Header("G³owa gracza")]
    public Transform head;

    void Awake()
    {
        if (!target)
            target = GetComponentInParent<EnemyHealth>();

        if (!head)
        {
            var cam = Camera.main;
            if (cam) head = cam.transform;
        }

        if (!head)
        {
            var anyCam = FindFirstObjectByType<Camera>();
            if (anyCam) head = anyCam.transform;
        }
    }

    void LateUpdate()
    {
        if (!target || target.IsDead)
        {
            gameObject.SetActive(false);
            return;
        }

        // obrót tylko na osi Y
        if (head)
        {
            Vector3 dir = transform.position - head.position;
            dir.y = 0f;

            if (dir.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(dir);
        }

        // HP %
        float hp01 = target.maxHealth > 0
            ? (float)target.currentHealth / target.maxHealth
            : 0f;

        if (hpBar)
            hpBar.localScale = new Vector3(Mathf.Clamp01(hp01), 1f, 1f);

        // ARMOR %
        bool hasArmor = target.maxArmor > 0;
        bool showArmor = hasArmor && target.currentArmor > 0;

        if (armorBackground)
            armorBackground.SetActive(showArmor);

        if (armorBar)
        {
            armorBar.gameObject.SetActive(showArmor);

            float armor01 = hasArmor
                ? (float)target.currentArmor / target.maxArmor
                : 0f;

            armorBar.localScale = new Vector3(Mathf.Clamp01(armor01), 1f, 1f);
        }
    }
}