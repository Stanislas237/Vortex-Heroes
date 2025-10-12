using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ShipController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 20f;          // Vitesse latérale et verticale
    public float tiltAmount = 30f;         // Angle d'inclinaison max
    public float tiltSmooth = 5f;          // Lissage du tilt

    [Header("Oscillation (ship shake)")]
    public float wobbleAmplitude = 0.3f;   // Amplitude du bougé naturel
    public float wobbleFrequency = 2f;     // Fréquence du bougé

    private Rigidbody rb;
    private Vector3 inputDir;
    private float wobbleTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = false;
    }

    void Update()
    {
        // --- INPUTS ---
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        inputDir = new Vector3(h, v, 0);

        // --- Inclinaison visuelle ---
        float tiltX = -v * tiltAmount;
        float tiltZ = -h * tiltAmount;

        Quaternion targetRot = Quaternion.Euler(tiltX, 0, tiltZ);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * tiltSmooth);

        // --- Oscillation naturelle ---
        wobbleTimer += Time.deltaTime * wobbleFrequency;
        float wobbleX = Mathf.Sin(wobbleTimer) * wobbleAmplitude;
        float wobbleY = Mathf.Cos(wobbleTimer * 0.7f) * wobbleAmplitude * 0.5f;

        transform.localPosition += new Vector3(wobbleX, wobbleY, 0) * Time.deltaTime;
    }

    void FixedUpdate()
    {
        // --- Déplacement principal ---
        Vector3 move = inputDir * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);
    }
}
