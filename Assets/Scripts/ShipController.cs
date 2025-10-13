using UnityEngine;
using UnityEngine.InputSystem;

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

    [Header("Oscillation (ship shake)")]
    public Material skyboxMaterial; // Matériau du skybox

    private PlayerInputActions playerInputActions;
    private Vector3 inputDir;
    private Rigidbody rb;
    private float wobbleTimer;
    
    void Awake() => playerInputActions = new();
    void OnEnable() => playerInputActions?.Enable();
    void OnDisable() => playerInputActions?.Disable();
    
    void Start()
    {
        // --- Initialisation du skybox ---
        if (skyboxMaterial != null)
        {
            RenderSettings.skybox = skyboxMaterial;
            DynamicGI.UpdateEnvironment();
        }

        // --- Ajout d'événements à l'InputActions ---
        playerInputActions.Player.Move.performed += ctx => inputDir = ctx.ReadValue<Vector2>();
        playerInputActions.Player.Move.canceled += ctx => inputDir = Vector3.zero;

        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = false;
    }

    void Update()
    {
        // --- Inclinaison visuelle ---
        float tiltX = -inputDir.y * tiltAmount;
        float tiltZ = -inputDir.x * tiltAmount;

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
