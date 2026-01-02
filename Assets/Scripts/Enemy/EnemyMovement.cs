using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public WaypointPath path;
    public Transform aimPoint;
    public float speed = 2f;
    public float reachDistance = 0.2f;
    public float enemyHeight = 1.6f;

    public Animator animator;

    [Header("Drabina")]
    public EnemyLadder ladderGoal;

    private int currentIndex = 0;

    public int CurrentIndex => currentIndex;

    private bool _isClimbing;
    private EnemyLadder _currentLadder;
    private float _climbTimer;

    private float _baseYaw;

    private bool _isExitingLadder;

    private bool _prevRootMotion;
    private bool _didMatchOnExit;
    void Start()
    {
        if (animator != null)
        {
            animator.Play("Locomotion", 0, Random.value);
            animator.Update(0f);
        }
    }


    void Update()
    {
        if (_isClimbing)
        {
            HandleClimb();
            return;
        }

        if (_isExitingLadder)
        {
            HandleLadderExit();
            return;
        }

        if (path == null || path.points.Length == 0) return;

        if (currentIndex < 0 || currentIndex >= path.points.Length)
        {
            return;
        }

        Transform targetPoint = path.points[currentIndex];

        // spód przeciwnika
        Vector3 bottom = transform.position + Vector3.down * (enemyHeight * 0.5f);

        // kierunek ruchu
        Vector3 toTarget = targetPoint.position - bottom;

        float moveSpeed = 0f;

        if (toTarget.sqrMagnitude > 0.0001f)
        {
            Vector3 dir = toTarget.normalized;

            // przesunięcie w kieruku kolejnego punktu na ścieżce
            transform.position += dir * speed * Time.deltaTime;
            moveSpeed = speed;

            // obrót w stronę kierunku ruchu
            Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5f);
        }

        // ustawianie animacji chodzenia
        if (animator != null)
        {
            animator.SetFloat("MoveSpeed", moveSpeed);
        }

        // sprawdzanie czy przeciwnik dotarł do celu
        if (Vector3.Distance(bottom, targetPoint.position) < reachDistance)
        {
            currentIndex++;
            if (currentIndex >= path.points.Length)
            {
                ReachedGoal();
            }
        }
    }

    // dotarcie do celu
    void ReachedGoal()
    {
        if (ladderGoal != null)
        {
            StartClimb(ladderGoal);
            return;
        }

        Destroy(gameObject);
    }

    // rozpoczęcie wspinaczki
    void StartClimb(EnemyLadder ladder)
    {
        _isClimbing = true;
        _currentLadder = ladder;
        _climbTimer = 0f;

        // kierunek przed wejściem na drabinę
        _baseYaw = transform.eulerAngles.y;

        if (_currentLadder.bottomPoint != null)
        {
            transform.position = _currentLadder.bottomPoint.position;

            Vector3 toBottom = (_currentLadder.bottomPoint.position - transform.position).normalized;

            Quaternion yawRot = Quaternion.Euler(0f, _baseYaw, 0f);
            if (toBottom.sqrMagnitude > 0.0001f)
            {
                Vector3 up0 = yawRot * Vector3.up;
                Vector3 right0 = yawRot * Vector3.right;

                float pitch = Vector3.SignedAngle(up0, -toBottom, right0);
                Quaternion tiltRot = Quaternion.AngleAxis(pitch, right0);

                transform.rotation = tiltRot * yawRot;
            }
            else
            {
                transform.rotation = yawRot;
            }
        }

        // animacja wspianczki
        if (animator != null)
            animator.SetBool("Climb", true);
    }

    // wspinaczka
    void HandleClimb()
    {
        if (_currentLadder == null || _currentLadder.bottomPoint == null || _currentLadder.topPoint == null)
        {
            _isClimbing = false;
            Destroy(gameObject);
            return;
        }

        _climbTimer += Time.deltaTime;
        float duration = Mathf.Max(0.01f, _currentLadder.climbDuration);
        float t = Mathf.Clamp01(_climbTimer / duration);

        Vector3 startPos = _currentLadder.bottomPoint.position;
        Vector3 endPos = _currentLadder.topPoint.position;

        transform.position = Vector3.Lerp(startPos, endPos, t);

        // obrót podczas wspinaczki żeby dół przeciwnika był skierowany w stronę dołu drabiny
        Vector3 toBottom = (_currentLadder.bottomPoint.position - transform.position).normalized;

        Quaternion yawRot = Quaternion.Euler(0f, _baseYaw, 0f);
        if (toBottom.sqrMagnitude > 0.0001f)
        {
            Vector3 up0 = yawRot * Vector3.up;
            Vector3 right0 = yawRot * Vector3.right;

            float pitch = Vector3.SignedAngle(up0, -toBottom, right0);
            Quaternion tiltRot = Quaternion.AngleAxis(pitch, right0);

            transform.rotation = tiltRot * yawRot;
        }
        else
        {
            transform.rotation = yawRot;
        }

        if (t >= 1f)
        {
            _isClimbing = false;

            // wyłączenie animacji wspinaczki
            if (animator != null)
                animator.SetBool("Climb", false);

            StartLadderExit();
        }
    }

    void StartLadderExit()
    {
        _isExitingLadder = true;
        _didMatchOnExit = false;

        if (animator != null)
        {
            _prevRootMotion = animator.applyRootMotion;
            animator.applyRootMotion = true;

            animator.SetTrigger("LadderExit");
        }
    }

    void HandleLadderExit()
    {
        if (_currentLadder == null)
        {
            _isExitingLadder = false;
            if (animator != null) animator.applyRootMotion = _prevRootMotion;
            Destroy(gameObject);
            return;
        }


        bool endByAnim = false;
        if (animator != null)
        {
            var st = animator.GetCurrentAnimatorStateInfo(0);
            if (st.IsName("LadderExit") && st.normalizedTime >= 1f) endByAnim = true;
        }

        if (endByAnim)
        {
            _isExitingLadder = false;
            if (animator != null) animator.applyRootMotion = _prevRootMotion;

            SnapToGround();

            var combat = GetComponent<EnemyCombatController>();
            if (combat != null)
            {
                combat.enabled = true;
                combat.BeginCombat();
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
    void OnAnimatorMove()
    {
        if (animator == null) return;

        if (_isExitingLadder)
        {
            var st = animator.GetCurrentAnimatorStateInfo(0);
            if (!_didMatchOnExit && st.IsName("LadderExit") && st.normalizedTime < 0.15f && _currentLadder != null)
            {
                var pos = _currentLadder.topPoint.position;

                var rot = transform.rotation;
                var weightMask = new MatchTargetWeightMask(new Vector3(1f, 0f, 1f), 0f);

                animator.MatchTarget(
                    pos,
                    rot,
                    AvatarTarget.Root,
                    weightMask,
                    0f,
                    0.15f
                );

                _didMatchOnExit = true;
            }

            transform.position += animator.deltaPosition;
            transform.rotation = animator.deltaRotation * transform.rotation;
        }
    }
    private void SnapToGround()
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;

        if (Physics.Raycast(origin, Vector3.down, out var hit, 2f))
        {
            Vector3 pos = transform.position;

            float halfHeight = enemyHeight * 0.5f;

            pos.y = hit.point.y + halfHeight;

            transform.position = pos;
        }
    }
}