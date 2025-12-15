using System.Collections.Generic;
using UnityEngine;

public class EnemyCombatController : MonoBehaviour
{
    [Header("Cel")]
    public Transform playerTarget;

    [Header("Ruch w walce")]
    public float combatMoveSpeed = 4f;
    public float backRowSpeedFactor = 0.3f;

    [Tooltip("Zasięg, w którym wróg może zacząć atak.")]
    public float attackRange = 1.6f;

    [Tooltip("Odległość pierwszego rzędu od gracza.")]
    public float stopDistance = 1.3f;

    [Tooltip("Dodatkowa odległość dla drugiego rzędu.")]
    public float backRowDistanceOffset = 1.2f;

    [Header("Odstęp między przeciwnikami")]
    [Tooltip("Minimalna odległość między przeciwnikami.")]
    public float minDistanceFromOtherEnemies = 1.0f;

    [Tooltip("Odstęp w bok od idealnej pozycji, żeby nie stali w kolejce.")]
    public float lateralOffset = 0.7f;

    [Header("Ataki")]
    public string[] attackStateNames;
    public float attackCrossfadeDuration = 0.1f;
    public float attackIntervalMin = 2f;
    public float attackIntervalMax = 4f;

    [Header("Broń")]
    public EnemyWeapon enemyWeapon;

    [Header("Reakcja na obrażenia")]
    public string hitTrigger = "Hit";
    public string stunTrigger = "Stun";
    public string stunEndTrigger = "StunEnd";
    public string hitSideParam = "HitSide";

    public float fleshStunDuration = 0.6f;
    public float armorStunDuration = 0.3f;

    [Range(0f, 1f)] public float fleshStunChance = 0.3f;
    [Range(0f, 1f)] public float armorStunChance = 0.2f;

    [SerializeField] private float moveSpeedDampTime = 0.25f;


    private bool _attackHitWindowActive;

    private bool _isStunned;
    private float _stunTimer;

    public bool IsStunned => _isStunned;

    private enum CombatState
    {
        Inactive,
        Approach,
        AttackLoop
    }

    private CombatState _state = CombatState.Inactive;
    private float _attackTimer;
    private float _currentAttackInterval;

    private EnemyMovement _movement;
    private Animator _animator;
    private EnemyHealth _health;
    private bool _isDead;

    private Collider _rootCollider;
    private Collider[] _childColliders;

    // wszystkie aktywne instancje tego skryptu
    private static readonly List<EnemyCombatController> _allEnemies =
        new List<EnemyCombatController>();

    // INIT
    void Awake()
    {
        _movement = GetComponent<EnemyMovement>();
        if (_movement != null)
            _animator = _movement.animator;

        if (_animator == null)
            _animator = GetComponentInChildren<Animator>();

        _health = GetComponent<EnemyHealth>();

        // domyślnie kamera jako cel
        if (playerTarget == null)
        {
            if (Camera.main != null)
            {
                playerTarget = Camera.main.transform;
            }
            else
            {
                var cam = FindObjectOfType<Camera>();
                if (cam != null)
                    playerTarget = cam.transform;
            }
        }

        // główny collider
        _rootCollider = GetComponent<Collider>();

        // collidery na dzieciach
        var allColliders = GetComponentsInChildren<Collider>(true);
        _childColliders = System.Array.FindAll(allColliders, c => c != _rootCollider);

        if (!_allEnemies.Contains(this))
            _allEnemies.Add(this);

            if (enemyWeapon == null)
        enemyWeapon = GetComponentInChildren<EnemyWeapon>();
    }

    void OnDestroy()
    {
        _allEnemies.Remove(this);
    }

    void OnDisable()
    {
        _allEnemies.Remove(this);
    }

    void Start()
    {
        _currentAttackInterval = Random.Range(attackIntervalMin, attackIntervalMax);
    }

    // helper do płynnego ustawiania parametru prędkości w animatorze
    private void SetMoveSpeed(float speed)
    {
        if (_animator == null) return;
        _animator.SetFloat("MoveSpeed", speed, moveSpeedDampTime, Time.deltaTime);
    }

    void Update()
    {
        if (_isDead) return;

        if (_health != null && _health.IsDead)
        {
            _isDead = true;
            _state = CombatState.Inactive;
            SetMoveSpeed(0f);
            return;
        }

        // stan ogłuszenia – brak ruchu i czekanie na koniec
        if (_isStunned)
        {
            _stunTimer -= Time.deltaTime;
            if (_stunTimer <= 0f)
            {
                _stunTimer = 0f;
                _isStunned = false;

                // koniec stuna – trigger do wyjścia ze stanu Stun w animatorze
                if (_animator != null && !string.IsNullOrEmpty(stunEndTrigger))
                    _animator.SetTrigger(stunEndTrigger);
            }

            SetMoveSpeed(0f);
            return;
        }

        if (_state == CombatState.Inactive) return;
        if (playerTarget == null) return;

        bool isAttackingNow = IsAttacking();

        if (enemyWeapon != null)
        {
            if (isAttackingNow && !_attackHitWindowActive)
            {
                _attackHitWindowActive = true;
                enemyWeapon.BeginHitWindow();
            }
            else if (!isAttackingNow && _attackHitWindowActive)
            {
                _attackHitWindowActive = false;
                enemyWeapon.EndHitWindow();
            }
        }

        if (isAttackingNow) return;

        switch (_state)
        {
            case CombatState.Approach:
                UpdateApproach();
                break;
            case CombatState.AttackLoop:
                UpdateAttackLoop();
                break;
        }
    }

    // przejście w tryb walki wręcz
    public void BeginCombat()
    {
        if (_isDead) return;
        if (playerTarget == null)
        {
            Debug.LogWarning("EnemyCombatController: Brak playerTarget.");
            return;
        }

        if (_movement != null)
            _movement.enabled = false;

        SetMeleeModeColliders();

        _state = CombatState.Approach;
        _attackTimer = 0f;
        _currentAttackInterval = Random.Range(attackIntervalMin, attackIntervalMax);

        SetMoveSpeed(0f);
    }

    public void StopCombat()
    {
        if (_movement != null)
            _movement.enabled = true;

        _state = CombatState.Inactive;
    }

    // ruch i ustawianie się wokół gracza
    private void UpdateApproach()
    {
        if (_isStunned) return;
        if (IsAttacking()) return;

        Vector3 enemyPos = transform.position;
        Vector3 playerPos = playerTarget.position;

        Vector3 toPlayer = playerPos - enemyPos;
        toPlayer.y = 0f;
        float dist = toPlayer.magnitude;
        if (dist < 0.001f) return;

        Vector3 dirToPlayer = toPlayer / dist;
        bool isFrontline = IsFrontline();

        // odległość od gracza dla pierwszego / drugiego rzędu
        float targetDistFromPlayer = isFrontline
            ? stopDistance
            : stopDistance + backRowDistanceOffset;

        // jeśli nikt jeszcze nie doszedł do gracza – ten wróg idzie prosto
        if (NoOneReachedPlayerYet())
        {
            Vector3 straightPos = playerPos - dirToPlayer * targetDistFromPlayer;
            MoveDirect(straightPos);
            return;
        }

        // bazowa pozycja przed graczem na linii wróg–gracz
        Vector3 basePos = playerPos - dirToPlayer * targetDistFromPlayer;

        // proste rozstrzelenie w bok na podstawie ID obiektu
        int hash = Mathf.Abs(GetInstanceID());
        int laneIndex = hash % 3; // 0,1,2

        float sideSign = 0f;
        if (laneIndex == 1) sideSign = 1f;
        else if (laneIndex == 2) sideSign = -1f;

        float lateralBase = lateralOffset;
        if (!isFrontline)
            lateralBase *= 0.7f;

        Vector3 sideDir = Vector3.Cross(Vector3.up, dirToPlayer);
        Vector3 desiredPos = basePos + sideDir * sideSign * lateralBase;

        // obcięcie do areny
        if (FightManager.Instance != null)
            desiredPos = FightManager.Instance.ClampToArena(desiredPos);

        // wybór najlepszego miejsca w okolicy – szukamy pozycji z dobrym odstępem od innych
        Vector3 bestPos = FindBestTargetPosition(desiredPos, dirToPlayer, sideDir, isFrontline);
        MoveDirect(bestPos);
    }

    // prosty ruch do wskazanego punktu na płaszczyźnie XZ
    private void MoveDirect(Vector3 targetPos)
    {
        Vector3 pos = transform.position;

        // trzymanie się płaszczyzny XZ
        targetPos.y = pos.y;

        Vector3 toTarget = targetPos - pos;
        toTarget.y = 0f;
        float dist = toTarget.magnitude;

        float speed = 0f;

        if (dist > 0.05f)
        {
            Vector3 moveDir = toTarget.normalized;
            Vector3 newPos = pos + moveDir * combatMoveSpeed * Time.deltaTime;
            newPos.y = pos.y;

            newPos = EnforceSeparation(newPos);

            if (FightManager.Instance != null)
                newPos = FightManager.Instance.ClampToArena(newPos);

            transform.position = newPos;
            speed = combatMoveSpeed;

            // obrót tylko po Y w kierunku ruchu / celu
            Vector3 flatLook = targetPos - newPos;
            flatLook.y = 0f;
            if (flatLook.sqrMagnitude > 0.0001f)
            {
                Quaternion rot = Quaternion.LookRotation(flatLook.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, rot, 10f * Time.deltaTime);
            }
        }

        SetMoveSpeed(speed);

        float distToPlayer = (playerTarget.position - transform.position).magnitude;
        if (distToPlayer <= attackRange && IsFrontline())
        {
            _state = CombatState.AttackLoop;
            _attackTimer = 0f;
            _currentAttackInterval = Random.Range(attackIntervalMin, attackIntervalMax);
        }
    }

    // tryb ataku – wróg stoi przy graczu i co jakiś czas wykonuje animację ataku
    private void UpdateAttackLoop()
    {
        if (_isStunned) return;
        if (IsAttacking()) return;

        Vector3 enemyPos = transform.position;
        Vector3 targetPos = playerTarget.position;

        Vector3 toTarget = targetPos - enemyPos;
        toTarget.y = 0f;
        float dist = toTarget.magnitude;

        // jeśli wróg się za bardzo oddalił lub przestał być w pierwszym rzędzie – wraca do podejścia
        if (dist > attackRange * 2f || !IsFrontline())
        {
            _state = CombatState.Approach;
            return;
        }

        if (toTarget.sqrMagnitude > 0.0001f)
        {
            Vector3 dirToPlayer = toTarget / Mathf.Max(dist, 0.0001f);
            Quaternion look = Quaternion.LookRotation(dirToPlayer, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * 10f);
        }

        SetMoveSpeed(0f);

        // odliczanie czasu do kolejnego ataku
        _attackTimer += Time.deltaTime;
        if (_attackTimer >= _currentAttackInterval)
        {
            _attackTimer = 0f;
            _currentAttackInterval = Random.Range(attackIntervalMin, attackIntervalMax);

            if (_animator != null && attackStateNames != null && attackStateNames.Length > 0)
            {
                int index = Random.Range(0, attackStateNames.Length);
                string stateName = attackStateNames[index];

                _animator.CrossFade(stateName, attackCrossfadeDuration, 0, 0f);
            }
        }
    }

    // reakcja na cios – wybór strony uderzenia
    public void PlayHitReaction(Vector3 worldHitDir)
    {
        if (_animator == null) return;
        if (_isDead) return;

        // awaryjny kierunek, gdyby przyszło coś bez długości
        if (worldHitDir.sqrMagnitude < 0.0001f)
            worldHitDir = -transform.forward;

        // przeliczenie na lokalny układ wroga
        Vector3 local = transform.InverseTransformDirection(worldHitDir);
        local.y = 0f;

        if (local.sqrMagnitude > 0.0001f)
            local.Normalize();

        float side = Mathf.Clamp(local.x, -1f, 1f);

        if (!string.IsNullOrEmpty(hitSideParam))
            _animator.SetFloat(hitSideParam, side);

        if (!string.IsNullOrEmpty(hitTrigger))
            _animator.SetTrigger(hitTrigger);
    }

    // ustawienie stuna – zatrzymanie ruchu i odpalenie animacji
    public void ApplyStun(float duration)
    {
        if (duration <= 0f) return;
        if (_isDead) return;

        _isStunned = true;
        _stunTimer = Mathf.Max(_stunTimer, duration);

        if (!string.IsNullOrEmpty(stunTrigger) && _animator != null)
            _animator.SetTrigger(stunTrigger);
    }

    // tryb walki wręcz – wyłączenie głównego collidra, włączenie childów
    private void SetMeleeModeColliders()
    {
        if (_rootCollider != null)
            _rootCollider.enabled = false;

        if (_childColliders == null) return;

        foreach (var col in _childColliders)
        {
            if (col == null) continue;
            col.enabled = true;
        }
    }

    // czy nikt jeszcze nie doszedł wystarczająco blisko gracza
    private bool NoOneReachedPlayerYet()
    {
        foreach (var e in _allEnemies)
        {
            if (e == null || e == this) continue;
            if (e._state != CombatState.Approach && e._state != CombatState.AttackLoop) continue;

            float dist = (e.transform.position - playerTarget.position).magnitude;
            if (dist <= stopDistance + 0.5f)
                return false;
        }
        return true;
    }

    // sprawdzenie czy aktualny stan animatora to któryś z ataków
    private bool IsAttacking()
    {
        if (_animator == null) return false;

        var st = _animator.GetCurrentAnimatorStateInfo(0);

        if (attackStateNames == null) return false;

        foreach (var s in attackStateNames)
            if (st.IsName(s))
                return true;

        return false;
    }

    // ranking po dystansie – ile wrogów jest bliżej gracza niż ten
    private int GetDistanceRank()
    {
        if (playerTarget == null) return int.MaxValue;

        Vector3 myPos = transform.position;
        Vector3 playerPos = playerTarget.position;

        float myDistSqr = (myPos - playerPos).sqrMagnitude;
        int rank = 0;

        foreach (var other in _allEnemies)
        {
            if (other == null || other == this) continue;
            if (other._state == CombatState.Inactive) continue;
            if (other.playerTarget == null) continue;

            float otherDistSqr = (other.transform.position - playerPos).sqrMagnitude;

            // mały margines, żeby nie przeskakiwali ciągle miejscami (~5 cm)
            if (otherDistSqr < myDistSqr - 0.05f * 0.05f)
                rank++;
        }

        return rank;
    }

    // czy wróg jest w pierwszym rzędzie (może atakować)
    private bool IsFrontline()
    {
        int maxAttackers = 3;
        if (FightManager.Instance != null)
            maxAttackers = Mathf.Max(1, FightManager.Instance.maxSimultaneousAttackers);

        int rank = GetDistanceRank();
        return rank < maxAttackers;
    }

    // wybór najlepszego miejsca obok gracza
    private Vector3 FindBestTargetPosition(Vector3 desiredPos, Vector3 dirToPlayer, Vector3 sideDir, bool isFrontline)
    {
        Vector2[] offsets =
        {
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(-1f, 0f),
            new Vector2(0f, 0.5f),
            new Vector2(0f, -0.5f),
            new Vector2(1f, 0.5f),
            new Vector2(-1f, 0.5f),
            new Vector2(1f, -0.5f),
            new Vector2(-1f, -0.5f),
        };

        float lateralBase = lateralOffset * (isFrontline ? 1f : 0.7f);
        float forwardStep = minDistanceFromOtherEnemies * 0.4f;

        Vector3 bestPos = desiredPos;
        float bestScore = float.NegativeInfinity;

        foreach (var off in offsets)
        {
            Vector3 candidate = desiredPos;
            candidate += sideDir * (off.x * lateralBase);
            candidate += (-dirToPlayer) * (off.y * forwardStep);

            if (FightManager.Instance != null)
                candidate = FightManager.Instance.ClampToArena(candidate);

            float minDist = ComputeMinDistanceToOthers(candidate);
            float distToDesired = (candidate - desiredPos).magnitude;

            // możliwie duży odstęp od innych z niewielką karą za oddalenie od pozycji idealnej
            float score = minDist - distToDesired * 0.3f;

            if (score > bestScore)
            {
                bestScore = score;
                bestPos = candidate;
            }
        }

        return bestPos;
    }

    // minimalny dystans do innych wrogów z danej pozycji
    private float ComputeMinDistanceToOthers(Vector3 pos)
    {
        float minDist = float.MaxValue;

        foreach (var other in _allEnemies)
        {
            if (other == null || other == this) continue;
            if (other._state == CombatState.Inactive) continue;

            Vector3 diff = pos - other.transform.position;
            diff.y = 0f;
            float dist = diff.magnitude;

            if (dist < minDist)
                minDist = dist;
        }

        if (minDist == float.MaxValue)
            minDist = 999f;

        return minDist;
    }

    // twarde pilnowanie minimalnego odstępu między przeciwnikami
    private Vector3 EnforceSeparation(Vector3 proposedPos)
    {
        Vector3 corrected = proposedPos;

        foreach (var other in _allEnemies)
        {
            if (other == null || other == this) continue;
            if (other._state == CombatState.Inactive) continue;

            Vector3 otherPos = other.transform.position;
            Vector3 diff = corrected - otherPos;
            diff.y = 0f;

            float dist = diff.magnitude;
            if (dist < 0.0001f) continue;

            if (dist < minDistanceFromOtherEnemies)
            {
                float push = (minDistanceFromOtherEnemies - dist);
                Vector3 pushDir = diff / dist;
                corrected += pushDir * push;
            }
        }

        corrected.y = proposedPos.y;
        return corrected;
    }
}