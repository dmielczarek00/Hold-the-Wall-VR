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

    // te metody wywo³ujesz z eventów animacji
    public void BeginHitWindow()
    {
        _hitWindowActive = true;
    }

    public void EndHitWindow()
    {
        _hitWindowActive = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_hitWindowActive) return;

        // globalny cooldown na dmg
        if (Time.time < _nextAllowedDamageTime) return;

        if (other.CompareTag(playerBodyTag))
        {
            // TODO: odbieranie HP graczowi

            Debug.Log($"EnemyWeapon: trafiono gracza za {damage}");

            _nextAllowedDamageTime = Time.time + damageCooldown;
        }
    }
}