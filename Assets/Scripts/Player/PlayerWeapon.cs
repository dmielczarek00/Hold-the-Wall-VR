using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlayerWeapon : MonoBehaviour
{
    [Header("Konfiguracja zamachu")]
    public float swingStartSpeed = 1.5f;
    public float swingEndSpeed = 0.8f;

    [Header("Obra¿enia")]
    public int baseDamage = 1;
    public int armorPenetration = 0;
    public int shred = 0;

    public float damageCooldown = 0.5f;

    [Header("DŸwiêki")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] slashSounds;

    private Vector3 _lastPosition;
    private Vector3 _velocity;
    private bool _isSwinging;

    private readonly Dictionary<EnemyHealth, float> _nextAllowedHitTime = new();

    public bool IsSwinging => _isSwinging;
    public Vector3 CurrentVelocity => _velocity;

    private void Start()
    {
        _lastPosition = transform.position;
        GetComponent<Collider>().isTrigger = true;
    }

    private void Update()
    {
        UpdateVelocity();
        UpdateSwingState();
    }

    // obliczanie prêdkoœci ruchu broni
    private void UpdateVelocity()
    {
        Vector3 pos = transform.position;
        float dt = Time.deltaTime;

        _velocity = dt > 0f ? (pos - _lastPosition) / dt : Vector3.zero;
        _lastPosition = pos;
    }

    // ustalanie, czy trwa zamach
    private void UpdateSwingState()
    {
        float speed = _velocity.magnitude;

        if (!_isSwinging && speed >= swingStartSpeed)
        {
            _isSwinging = true;
            return;
        }

        if (_isSwinging && speed < swingEndSpeed)
        {
            _isSwinging = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_isSwinging) return;

        // strefa trafienia na przeciwniku
        EnemyHitZones hitZones = other.GetComponentInParent<EnemyHitZones>();
        if (hitZones == null) return;

        if (!hitZones.TryResolveHit(other, out EnemyHitZone zone)) return;

        EnemyHealth health = hitZones.health;
        if (health == null) return;

        // ignorujemy zderzenia z broni¹ przeciwnika
        if (zone == EnemyHitZone.Weapon)
            return;

        // cooldown na danym celu
        if (_nextAllowedHitTime.TryGetValue(health, out float nextTime) &&
            Time.time < nextTime)
            return;

        int dmg = baseDamage;
        int ap = armorPenetration;
        int sh = shred;

        // trafienie w g³owê
        if (zone == EnemyHitZone.Head)
            dmg = Mathf.RoundToInt(baseDamage * 2f);

        health.TakeDamage(dmg, ap, sh);

        // reakcja przeciwnika
        EnemyCombatController combat = health.GetComponent<EnemyCombatController>();
        if (combat != null && combat.enabled && !health.IsDead)
        {
            // kierunek ruchu broni
            Vector3 swingVel = _velocity;

            if (swingVel.sqrMagnitude < 0.0001f)
                swingVel = transform.position - health.transform.position;

            Vector3 hitDir = -swingVel.normalized;

            AudioPlay.PlaySound(audioSource, slashSounds);
            combat.PlayHitReaction(hitDir);

            float stunDuration = 0f;
            float stunChance = 0f;

            switch (zone)
            {
                case EnemyHitZone.Head:
                case EnemyHitZone.Torso:
                case EnemyHitZone.Body:
                    stunDuration = combat.fleshStunDuration;
                    stunChance = combat.fleshStunChance;
                    break;

                case EnemyHitZone.Armor:
                    stunDuration = combat.armorStunDuration;
                    stunChance = combat.armorStunChance;
                    break;
            }

            if (stunDuration > 0f && stunChance > 0f && Random.value <= stunChance)
                combat.ApplyStun(stunDuration);
        }


        // zapis czasu kolejnego mo¿liwego trafienia
        _nextAllowedHitTime[health] = Time.time + damageCooldown;
    }
}