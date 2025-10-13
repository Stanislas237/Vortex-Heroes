using UnityEngine;

public class CameraTPS : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Offset & Follow")]
    public bool useInitialOffset = true;
    public Vector3 offset = new(0, 3f, -10f);
    public float followSpeed = 5f;
    public float rotationSmooth = 2f;

    [Header("Shake & Dynamics")]
    public float shakeIntensity = 0.2f;
    public float shakeSpeed = 15f;

    private Vector3 currentVelocity;
    private Vector3 shakeOffset;

    void Start()
    {
        if (target && useInitialOffset)
            offset = transform.position - target.position;
    }

    void LateUpdate()
    {
        if (!target) return;

        // --- Position de base ---
        Vector3 desiredPos = target.TransformPoint(offset);

        // --- Tremblement dynamique (léger, effet vitesse) ---
        shakeOffset.x = Mathf.PerlinNoise(Time.time * shakeSpeed, 0f) - 0.5f;
        shakeOffset.y = Mathf.PerlinNoise(0f, Time.time * shakeSpeed) - 0.5f;
        shakeOffset *= shakeIntensity;

        desiredPos += shakeOffset;

        // --- Smooth follow ---
        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref currentVelocity, 1f / followSpeed);

        // --- Smooth rotation (regarde le vaisseau) ---
        Quaternion desiredRot = Quaternion.LookRotation(target.position + Vector3.up * offset.y - transform.position + target.forward * 2f);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, Time.deltaTime * rotationSmooth);
    }
}
