using UnityEngine;

public class HandModeBarFollower : MonoBehaviour
{
    [Tooltip("Kamera gracza (XR Camera)")]
    public Transform cameraTransform;

    [Header("Pozycja")]
    public float distance = 1.5f;
    public float heightOffset = -1.6f;

    [Header("Zachowanie tu³owia")]
    [Tooltip("O ile stopni musi siê obróciæ g³owa, ¿eby pasek zacz¹³ siê dostosowywaæ.")]
    public float yawThreshold = 20f;

    [Tooltip("Jak szybko tu³ów dogania g³owê.")]
    public float yawFollowSpeed = 180f;

    private float _bodyYaw;

    void Start()
    {
        if (cameraTransform != null)
        {
            _bodyYaw = cameraTransform.eulerAngles.y;
        }
    }

    void LateUpdate()
    {
        if (cameraTransform == null)
            return;

        float camYaw = cameraTransform.eulerAngles.y;
        float deltaYaw = Mathf.DeltaAngle(_bodyYaw, camYaw);

        if (Mathf.Abs(deltaYaw) > yawThreshold)
        {
            float maxStep = yawFollowSpeed * Time.deltaTime;
            float step = Mathf.Clamp(deltaYaw, -maxStep, maxStep);
            _bodyYaw += step;
        }

        Vector3 bodyForward = Quaternion.Euler(0f, _bodyYaw, 0f) * Vector3.forward;

        Vector3 basePos = cameraTransform.position;
        basePos.y += heightOffset;

        Vector3 targetPos = basePos + bodyForward * distance;

        transform.position = targetPos;

        transform.rotation = Quaternion.LookRotation(bodyForward, Vector3.up);
    }
}