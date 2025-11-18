using System.Collections.Generic;
using UnityEngine;

public class EnemyCombatController : MonoBehaviour
{
    [Header("Cel")]
    public Transform playerTarget;

    [Header("Ruch w walce")]
    public float combatMoveSpeed = 2f;
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

    private Collider _rootCollider;
    private Collider[] _childColliders;

    // wszystkie aktywne instancje tego skryptu
    private static readonly List<EnemyCombatController> _allEnemies =
        new List<EnemyCombatController>();

    void Awake()
    {
        _movement = GetComponent<EnemyMovement>();
        if (_movement != null)
            _animator = _movement.animator;

        if (_animator == null)
            _animator = GetComponentInChildren<Animator>();

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

    void Update()
    {
        if (_state == CombatState.Inactive) return;
        if (playerTarget == null) return;

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
        if (playerTarget == null)
        {
            Debug.LogWarning("EnemyCombatController: Brak playerTarget.");
            return;
        }

        SetMeleeModeColliders();

        _state = CombatState.Approach;
        _attackTimer = 0f;
        _currentAttackInterval = Random.Range(attackIntervalMin, attackIntervalMax);

        if (_animator != null)
            _animator.SetFloat("MoveSpeed", 0f);
    }

    public void StopCombat()
    {
        _state = CombatState.Inactive;
    }

    // ruch i ustawianie się wokół gracza
    private void UpdateApproach()
    {
        Vector3 enemyPos = transform.position;
        Vector3 playerPos = playerTarget.position;

        Vector3 toPlayer = playerPos - enemyPos;
        toPlayer.y = 0f;
        float distToPlayer = toPlayer.magnitude;
        if (distToPlayer < 0.001f) return;

        Vector3 dirToPlayer = toPlayer / distToPlayer;
        bool isFrontline = IsFrontline();

        // odległość od gracza dla pierwszego / drugiego rzędu
        float targetDistFromPlayer = isFrontline
            ? stopDistance
            : stopDistance + backRowDistanceOffset;

        // bazowa pozycja przed graczem na linii wróg–gracz
        Vector3 basePos = playerPos - dirToPlayer * targetDistFromPlayer;

        // proste rozstrzelenie w bok na podstawie ID obiektu
        int hash = Mathf.Abs(GetInstanceID());
        int laneIndex = hash % 3; // 0,1,2

        float sideSign = 0f;
        if (laneIndex == 1) sideSign = 1f;
        else if (laneIndex == 2) sideSign = -1f;

        float lateral = lateralOffset;
        if (!isFrontline)
            lateral *= 0.7f;

        Vector3 sideDir = Vector3.Cross(Vector3.up, dirToPlayer);
        Vector3 desiredPos = basePos + sideDir * sideSign * lateral;

        // obcięcie do areny
        if (FightManager.Instance != null)
            desiredPos = FightManager.Instance.ClampToArena(desiredPos);

        // wybór najlepszego miejsca w okolicy – szukamy pozycji z dobrym odstępem od innych
        Vector3 bestPos = FindBestTargetPosition(desiredPos, dirToPlayer, sideDir, isFrontline);

        Vector3 toTargetPos = bestPos - enemyPos;
        toTargetPos.y = 0f;
        float distToTargetPos = toTargetPos.magnitude;

        float moveSpeed = 0f;

        if (distToTargetPos > 0.05f)
        {
            // kierunek ruchu do wybranej pozycji
            Vector3 moveDirToTarget = toTargetPos.normalized;
            float speed = isFrontline ? combatMoveSpeed : combatMoveSpeed * backRowSpeedFactor;

            Vector3 newPos = enemyPos + moveDirToTarget * speed * Time.deltaTime;
            newPos.y = enemyPos.y;

            // pilnowanie minimalnego odstępu od innych
            newPos = EnforceSeparation(newPos);

            if (FightManager.Instance != null)
                newPos = FightManager.Instance.ClampToArena(newPos);

            transform.position = newPos;
            moveSpeed = speed;

            // w ruchu patrzymy w kierunku poruszania się
            if (moveDirToTarget.sqrMagnitude > 0.0001f)
            {
                Quaternion lookRot = Quaternion.LookRotation(moveDirToTarget, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 10f);
            }
        }
        else
        {
            // przy małym ruchu po prostu patrzy na gracza
            Quaternion lookRot = Quaternion.LookRotation(dirToPlayer, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 10f);
        }

        // animacja chodzenia / stania
        if (_animator != null)
            _animator.SetFloat("MoveSpeed", moveSpeed, 0.5f, Time.deltaTime);

        // wejście w tryb ataku tylko dla pierwszego rzędu w zasięgu
        if (isFrontline && distToPlayer <= attackRange)
        {
            _state = CombatState.AttackLoop;
            _attackTimer = 0f;
            _currentAttackInterval = Random.Range(attackIntervalMin, attackIntervalMax);
        }
    }

    // tryb ataku – wróg stoi przy graczu i co jakiś czas robi animację ataku
    private void UpdateAttackLoop()
    {
        Vector3 enemyPos = transform.position;
        Vector3 targetPos = playerTarget.position;

        Vector3 toTarget = targetPos - enemyPos;
        toTarget.y = 0f;
        float dist = toTarget.magnitude;

        if (toTarget.sqrMagnitude > 0.0001f)
        {
            Vector3 dirToPlayer = toTarget / Mathf.Max(dist, 0.0001f);

            // jak się oddali to wraca do podchodzenia
            float hardExitDistance = attackRange * 2.0f;
            if (dist > hardExitDistance)
            {
                _state = CombatState.Approach;
                return;
            }

            // jeśli przestał być w pierwszym rzędzie wraca do podejścia
            if (!IsFrontline())
            {
                _state = CombatState.Approach;
                return;
            }

            // delikatne korygowanie dystansu w miejscu
            float desiredDist = stopDistance;
            float band = 0.3f;
            float moveSpeed = 0f;
            Vector3 move = Vector3.zero;

            if (dist > desiredDist + band)
            {
                // odrobinę podejdź do gracza
                Vector3 moveDir = dirToPlayer;
                move = moveDir * (combatMoveSpeed * 0.5f * Time.deltaTime);
                moveSpeed = combatMoveSpeed * 0.5f;
            }
            else if (dist < desiredDist - band)
            {
                // odrobinę odsuń się od gracza
                Vector3 moveDir = -dirToPlayer;
                move = moveDir * (combatMoveSpeed * 0.5f * Time.deltaTime);
                moveSpeed = combatMoveSpeed * 0.5f;
            }

            if (move != Vector3.zero)
            {
                Vector3 newPos = enemyPos + move;
                newPos.y = enemyPos.y;

                newPos = EnforceSeparation(newPos);

                if (FightManager.Instance != null)
                    newPos = FightManager.Instance.ClampToArena(newPos);

                transform.position = newPos;
            }

            // animacja – lekki ruch lub pełne stanie
            if (_animator != null)
                _animator.SetFloat("MoveSpeed", moveSpeed, 0.3f, Time.deltaTime);

            // w trybie ataku zawsze patrzymy na gracza
            Quaternion look = Quaternion.LookRotation(dirToPlayer, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * 10f);
        }

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

            // mały margines, żeby nie przeskakiwali ciągle miejscami
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

            // możliwie duży odstęp od innych
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


    // tryb walki wręcz
    void SetMeleeModeColliders()
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
}