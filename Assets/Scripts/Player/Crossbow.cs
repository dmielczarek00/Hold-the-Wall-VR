using UnityEngine;
using UnityEngine.InputSystem;

public class Crossbow : MonoBehaviour
{
    [Header("Wejœcia XR")]
    public InputActionReference fireAction;

    [Header("Parametry strza³u")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 30f;
    public float projectileLifeTime = 10f;
    public int damage = 1;

    [Header("Cooldown")]
    [Tooltip("Minimalna iloœæ sekund miêdzy strza³ami.")]
    public float fireCooldown = 1f;

    [Header("Miejsca na obiekcie")]
    public Transform firePoint;

    [Header("DŸwiêki")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] fireSounds;

    private bool _firePressed;
    private float nextFireTime = 0f;

    void OnEnable()
    {
        if (fireAction?.action != null)
            fireAction.action.performed += OnFirePerformed;
    }

    void OnDisable()
    {
        if (fireAction?.action != null)
            fireAction.action.performed -= OnFirePerformed;
    }

    void OnFirePerformed(InputAction.CallbackContext ctx)
    {
        _firePressed = true;
    }

    void Update()
    {
        if (_firePressed)
        {
            _firePressed = false;
            TryShoot();
        }
    }

    private void TryShoot()
    {
        if (Time.time < nextFireTime)
            return;

        Shoot();
        nextFireTime = Time.time + fireCooldown;
    }

    private void Shoot()
    {
        if (projectilePrefab == null || firePoint == null)
            return;

        GameObject obj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Projectile proj = obj.GetComponent<Projectile>();

        if (proj != null)
        {
            var payload = new DamagePayload
            {
                amount = damage,
                armorPenetration = 5,
                shred = 1,
                source = gameObject
            };

            Vector3 targetPoint = firePoint.position + firePoint.forward * 999f;

            AudioPlay.PlaySound(audioSource, fireSounds);
            proj.Initialize(targetPoint, projectileSpeed, projectileLifeTime, payload);
        }
    }
}