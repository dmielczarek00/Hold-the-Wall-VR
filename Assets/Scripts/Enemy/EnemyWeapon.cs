using UnityEngine;

public class EnemyWeapon : MonoBehaviour
{
    [Header("Broñ przeciwnika")]
    public float damage = 10f;
    public string playerBodyTag = "PlayerBody";

    [Tooltip("Minimalny czas miêdzy kolejnymi trafieniami (globalnie dla broni).")]
    public float damageCooldown = 0.5f;

    private bool _hitWindowActive;
    private float _nextAllowedDamageTime;
    private bool _hasHitInThisWindow;

    public void BeginHitWindow()
    {
        _hitWindowActive = true;
        _hasHitInThisWindow = false;
    }

    public void EndHitWindow()
    {
        _hitWindowActive = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_hitWindowActive) return;
        if (_hasHitInThisWindow) return;
        if (Time.time < _nextAllowedDamageTime) return;

        if (!other.CompareTag(playerBodyTag)) return;

        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
        if (health == null) return;

        health.TakeDamage(damage);

        _hasHitInThisWindow = true;
        _nextAllowedDamageTime = Time.time + damageCooldown;
    }
}