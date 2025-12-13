using UnityEngine;
using UnityEngine.InputSystem;

public class PlatformPlacementController : MonoBehaviour
{
    [Header("Wejœcia XR")]
    [Tooltip("Otwieranie menu.")]
    public InputActionReference openMenuAction;

    [Tooltip("Zatwierdzanie.")]
    public InputActionReference confirmAction;

    [Tooltip("Wy³¹czanie menu.")]
    public InputActionReference cancelAction;

    [Tooltip("Obracanie platformy.")]
    public InputActionReference rotateStickAction;

    [Header("UI")]
    public PlatformRadialMenu platformMenuPrefab;

    [Header("Raycast")]
    public Transform rayOrigin;
    public float maxRayDistance = 50f;
    public LayerMask placementLayerMask = ~0;

    [Header("Ghost")]
    public Material validMaterial;
    public Material invalidMaterial;

    [Header("Obracanie ghosta")]
    [Tooltip("Prêdkoœæ obrotu ghosta dr¹¿kiem.")]
    public float rotateSpeed = 90f;
    [Tooltip("Martwa strefa dr¹¿ka, poni¿ej której nie reaguje.")]
    public float rotateDeadzone = 0.2f;

    [Header("Blokowanie obrotu gracza")]
    [Tooltip("Komponent od obrotu gracza (snap/continuous turn). Wy³¹czany w trybie stawiania.")]
    public Behaviour turnProviderToDisable;

    private PlatformData _currentPlatform;
    private GameObject _ghostInstance;
    private bool _isPlacing;
    private bool _canPlaceHere;

    private Vector3 _snapLocalPosition;
    private string _currentAllowedTag;
    private float _footprintRadius;

    private bool _confirmPressed;
    private bool _cancelPressed;

    void OnEnable()
    {
        // aktywacja wejœæ XR
        SubscribeActions();
        //EnableActions();
    }

    void OnDisable()
    {
        // czyszczenie wejœæ i ghosta przy wy³¹czeniu
        UnsubscribeActions();
        //DisableActions();
        CancelPlacement();
    }

    void Update()
    {
        // dzia³a tylko w trybie stawiania
        if (_isPlacing)
        {
            HandlePlacementUpdate(); // pozycjonowanie ghosta
            HandleRotationInput();   // obracanie dr¹¿kiem
            HandlePlacementInput();  // potwierdzenie / anulowanie
        }
    }

    // podpina akcje XR
    void SubscribeActions()
    {
        if (openMenuAction?.action != null)
            openMenuAction.action.performed += OnOpenMenuPerformed;

        if (confirmAction?.action != null)
            confirmAction.action.performed += OnConfirmPerformed;

        if (cancelAction?.action != null)
            cancelAction.action.performed += OnCancelPerformed;
    }

    // wypina akcje XR
    void UnsubscribeActions()
    {
        if (openMenuAction?.action != null)
            openMenuAction.action.performed -= OnOpenMenuPerformed;

        if (confirmAction?.action != null)
            confirmAction.action.performed -= OnConfirmPerformed;

        if (cancelAction?.action != null)
            cancelAction.action.performed -= OnCancelPerformed;
    }

    void EnableActions()
    {
        EnableAction(openMenuAction);
        EnableAction(confirmAction);
        EnableAction(cancelAction);
        EnableAction(rotateStickAction);
    }

    void DisableActions()
    {
        DisableAction(openMenuAction);
        DisableAction(confirmAction);
        DisableAction(cancelAction);
        DisableAction(rotateStickAction);
    }

    void EnableAction(InputActionReference actionRef)
    {
        if (actionRef?.action != null && !actionRef.action.enabled)
            actionRef.action.Enable();
    }

    void DisableAction(InputActionReference actionRef)
    {
        if (actionRef?.action != null && actionRef.action.enabled)
            actionRef.action.Disable();
    }

    // naciœniêcie przycisku otwieraj¹cego menu
    void OnOpenMenuPerformed(InputAction.CallbackContext ctx)
    {
        if (_isPlacing) return;
        OpenPlatformMenu();
    }

    // potwierdzenie (np. trigger)
    void OnConfirmPerformed(InputAction.CallbackContext ctx)
    {
        if (_isPlacing)
            _confirmPressed = true;
    }

    // anulowanie (np. B)
    void OnCancelPerformed(InputAction.CallbackContext ctx)
    {
        if (_isPlacing)
            _cancelPressed = true;
    }

    // otwiera menu wyboru platform
    void OpenPlatformMenu()
    {

        // zamyka inne menu jeœli jest otwarte
        PlatformRadialMenu.CloseAll();

        // pozycja menu przed graczem
        var cam = Camera.main;
        Vector3 pos = cam ? cam.transform.position + cam.transform.forward * platformMenuPrefab.menuDistance : transform.position;
        Quaternion rot = cam ? Quaternion.LookRotation(cam.transform.forward, Vector3.up) : Quaternion.identity;

        var menu = Instantiate(platformMenuPrefab, pos, rot);
        menu.Init(this);
    }

    // uruchamia tryb stawiania po wybraniu platformy z menu
    public void BeginPlacement(PlatformData data)
    {
        if (data == null) return;

        CleanupGhost();

        _currentPlatform = data;
        _isPlacing = true;
        _confirmPressed = false;
        _cancelPressed = false;

        _currentAllowedTag = string.IsNullOrEmpty(data.allowedTag) ? "" : data.allowedTag;
        _footprintRadius = Mathf.Max(0f, data.footprintRadius);

        // wy³¹cza obracanie gracza w tym czasie
        if (turnProviderToDisable != null)
            turnProviderToDisable.enabled = false;

        CreateGhost();
    }

    // tworzy podgl¹d platformy (ghost)
    void CreateGhost()
    {
        if (_currentPlatform == null) return;

        GameObject prefab = _currentPlatform.ghostPrefab != null ? _currentPlatform.ghostPrefab : _currentPlatform.prefab;
        if (prefab == null) return;

        _ghostInstance = Instantiate(prefab);
        _ghostInstance.name = prefab.name + "_Ghost";

        // wy³¹cza kolizje i build spoty w ghostcie
        foreach (var col in _ghostInstance.GetComponentsInChildren<Collider>())
            col.enabled = false;

        foreach (var spot in _ghostInstance.GetComponentsInChildren<BuildSpot>())
            spot.enabled = false;

        // punkt, który ma dotykaæ ziemi
        var snapPoint = _ghostInstance.GetComponentInChildren<PlatformSnapPoint>();
        _snapLocalPosition = snapPoint ? _ghostInstance.transform.InverseTransformPoint(snapPoint.transform.position) : Vector3.zero;

        // startowa rotacja dopasowana do gracza
        float yRot = Camera.main ? Camera.main.transform.eulerAngles.y : 0f;
        _ghostInstance.transform.rotation = Quaternion.Euler(0f, yRot, 0f);
    }

    // aktualizuje pozycjê ghosta
    void HandlePlacementUpdate()
    {
        if (_ghostInstance == null || _currentPlatform == null) return;
        if (rayOrigin == null) return;

        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, placementLayerMask, QueryTriggerInteraction.Ignore))
        {
            bool tagOk = string.IsNullOrEmpty(_currentAllowedTag) || hit.collider.CompareTag(_currentAllowedTag);

            Vector3 offset = _ghostInstance.transform.rotation * _snapLocalPosition;
            Vector3 centerPos = hit.point - offset;

            _ghostInstance.transform.position = centerPos;

            bool insideArea = tagOk && IsInsideBuildAreaXZ(centerPos, _footprintRadius);
            _canPlaceHere = insideArea;

            UpdateGhostVisual(insideArea);
        }
        else
        {
            _canPlaceHere = false;
            UpdateGhostVisual(false);
        }
    }

    // sprawdza czy platforma mieœci siê w BuildArea (tylko XZ)
    bool IsInsideBuildAreaXZ(Vector3 center, float radius)
    {
        if (radius <= 0f) return true;

        float checkHeight = center.y + 5f;
        float maxDownDistance = 20f;
        Vector3 basePos = new Vector3(center.x, checkHeight, center.z);

        // próbki w czterech kierunkach od œrodka
        Vector2[] dirs =
        {
            new Vector2(1f, 0f),
            new Vector2(-1f, 0f),
            new Vector2(0f, 1f),
            new Vector2(0f, -1f)
        };

        for (int i = 0; i < dirs.Length; i++)
        {
            Vector2 d = dirs[i];
            Vector3 sample = basePos + new Vector3(d.x, 0f, d.y) * radius;

            // jeœli punkt nie trafia w BuildArea — platforma za du¿a
            if (!Physics.Raycast(sample, Vector3.down, out RaycastHit hit, maxDownDistance, placementLayerMask, QueryTriggerInteraction.Ignore))
                return false;

            if (!string.IsNullOrEmpty(_currentAllowedTag) && !hit.collider.CompareTag(_currentAllowedTag))
                return false;
        }

        return true;
    }

    // zmienia materia³ ghosta w zale¿noœci od mo¿liwoœci postawienia
    void UpdateGhostVisual(bool canPlace)
    {
        if (_ghostInstance == null) return;

        var rends = _ghostInstance.GetComponentsInChildren<MeshRenderer>();
        Material targetMat = canPlace ? validMaterial : invalidMaterial;
        if (targetMat == null) return;

        foreach (var r in rends)
            r.sharedMaterial = targetMat;
    }

    // obracanie platformy prawym dr¹¿kiem
    void HandleRotationInput()
    {
        if (_ghostInstance == null) return;
        if (rotateStickAction == null || rotateStickAction.action == null) return;

        Vector2 axis = rotateStickAction.action.ReadValue<Vector2>();
        float x = axis.x;

        if (Mathf.Abs(x) < rotateDeadzone)
            return;

        float deltaYaw = x * rotateSpeed * Time.deltaTime;

        Vector3 e = _ghostInstance.transform.rotation.eulerAngles;
        e.y += deltaYaw;
        _ghostInstance.transform.rotation = Quaternion.Euler(0f, e.y, 0f);
    }

    // obs³uguje klikniêcia zatwierdzenia / anulowania
    void HandlePlacementInput()
    {
        if (_cancelPressed)
        {
            _cancelPressed = false;
            CancelPlacement();
            return;
        }

        if (_confirmPressed)
        {
            _confirmPressed = false;
            TryPlacePlatform();
        }
    }

    // próbuje postawiæ platformê
    void TryPlacePlatform()
    {
        if (!_canPlaceHere || _currentPlatform == null || _ghostInstance == null) return;

        // sprawdza czy gracza staæ na platformê
        if (GameEconomy.I != null && !GameEconomy.I.TrySpend(_currentPlatform.cost))
        {
            return;
        }

        GameObject placed = Instantiate(_currentPlatform.prefab, _ghostInstance.transform.position, _ghostInstance.transform.rotation);
        placed.name = _currentPlatform.prefab.name;

        CancelPlacement();
    }

    // koñczy tryb stawiania i sprz¹ta po sobie
    public void CancelPlacement()
    {
        _isPlacing = false;
        _currentPlatform = null;
        _canPlaceHere = false;
        _confirmPressed = false;
        _cancelPressed = false;

        if (turnProviderToDisable != null)
            turnProviderToDisable.enabled = true;

        CleanupGhost();
    }

    // usuwa ghosta z gry
    void CleanupGhost()
    {
        if (_ghostInstance != null)
        {
            Destroy(_ghostInstance);
            _ghostInstance = null;
        }
    }
}