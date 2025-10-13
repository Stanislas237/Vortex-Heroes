using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class ShipCollisionHandler : MonoBehaviour
{
    [Header("Collision Reaction")]
    public float knockbackForce = 50f;
    public float destabilizeDuration = 1.5f;
    public float spinIntensity = 80f;

    private Rigidbody rb;
    private bool isDestabilized = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void CollisionHandler(Vector3 collisionPoint)
    {
        if (isDestabilized) return;

        Vector3 forceDir = (transform.position - collisionPoint).normalized;
        rb.AddForce(forceDir * knockbackForce, ForceMode.Impulse);
        StartCoroutine(DestabilizeRoutine());
        StartCoroutine(CameraShakeRoutine());
    }

    IEnumerator DestabilizeRoutine()
    {
        isDestabilized = true;

        float timer = 0f;
        while (timer < destabilizeDuration)
        {
            // Petites rotations folles (perte de contrôle)
            transform.Rotate(
                Random.Range(-spinIntensity, spinIntensity) * Time.deltaTime,
                Random.Range(-spinIntensity, spinIntensity) * Time.deltaTime,
                Random.Range(-spinIntensity, spinIntensity) * Time.deltaTime
            );

            timer += Time.deltaTime;
            yield return null;
        }

        isDestabilized = false;
    }

    IEnumerator CameraShakeRoutine()
    {
        var cameraFollow = CameraTPS.Instance;
        if (cameraFollow == null) yield break;

        float baseShake = cameraFollow.shakeIntensity;
        cameraFollow.shakeIntensity *= 4f; // plus fort pendant le choc
        yield return new WaitForSeconds(0.7f);
        cameraFollow.shakeIntensity = baseShake;
    }
}
