using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class ShipCollisionHandler : MonoBehaviour
{
    [Header("Collision Reaction")]
    public float knockbackForce = 50f;
    public float destabilizeDuration = 1.5f;
    public float restabilizeDuration = .5f;
    public float spinIntensity = 80f;

    [HideInInspector]
    public bool isDestabilized = false;

    private Rigidbody rb;

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

        while (timer < restabilizeDuration + destabilizeDuration)
        {
            // Petites rotations folles (perte de contrôle)
            var t = (timer - destabilizeDuration) / restabilizeDuration;
            rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, Vector3.zero, t);
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, t);
            timer += Time.deltaTime;
            yield return null;
        }

        rb.angularVelocity = Vector3.zero; // Stopper toute rotation
        rb.linearVelocity = Vector3.zero; // Stopper tout mouvement
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
