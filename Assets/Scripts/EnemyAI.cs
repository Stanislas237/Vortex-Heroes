using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyAI : MonoBehaviour
{
    [HideInInspector]
    public Transform player;

    [Header("Movement")]
    public float baseSpeed = 60f;
    public float speedVariation = 20f;
    public float stopDistance = 100f;
    public float followDistance = 500f;
    public float smoothTurn = 3f;

    [Header("Agitation / Drift")]
    public float wobbleAmplitude = 1.5f;
    public float wobbleFrequency = 5f;
    private float wobbleTimer;
    
    [Header("Lose")]
    public int enemyStopingDistanceAfterLose = -30;

    [Header("Obstacle Avoidance")]
    public LayerMask obstacleMask;
    public Vector2 posThreshold;
    public float avoidTimer = .5f;
    public float avoidCooldown = 1.5f;
    public float detectRadius = 50f;

    // Movement
    private Transform mainCamera;
    private Rigidbody rb;
    private float speed;
        
    // Obstacle Avoidance
    private Vector3 avoidDir;
    private bool avoiding;
    private float lastAvoidTime;
    

    public void Init(Transform playerTransform, Vector2 startPos, LayerMask obsMask)
    {
        player = playerTransform;
        posThreshold = startPos;
        obstacleMask = obsMask;
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = false;
        mainCamera = Camera.main.transform;

        // Vitesse initiale légèrement aléatoire
        speed = baseSpeed + Random.Range(-speedVariation, speedVariation);
    }

    void FixedUpdate()
    {
        if (!player)
        {
            rb.linearVelocity = transform.position.z > enemyStopingDistanceAfterLose ? Vector3.back * speed : Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            return;
        }

        Vector3 targetPos = player.position + (Vector3)posThreshold + player.forward * stopDistance;
        float distance = (targetPos - transform.position).magnitude;

        // --- Obstacle detection ---
        if (avoiding && Time.time - lastAvoidTime > avoidTimer)
        {
            avoiding = false;
            avoidDir = Vector3.zero;
        }

        if (Time.time - lastAvoidTime > avoidCooldown)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, detectRadius, obstacleMask);

            if (hits.Length > 0)
            {
                Vector3 totalAvoid = transform.position - hits[0].transform.position;
                if (totalAvoid != Vector3.zero)
                {
                    totalAvoid.Normalize();

                    // On tourne légèrement à gauche ou droite (style dogfight)
                    float side = Random.value > 0.5f ? 90f : -90f;
                    avoidDir = (Quaternion.Euler(0, side, 0) * totalAvoid).normalized;
                    // avoidDir.z = 0;
                    avoiding = true;
                    lastAvoidTime = Time.time;
                }
            }
        }

        // --- Drift naturel / agitation ---
        wobbleTimer += Time.fixedDeltaTime * wobbleFrequency;
        Vector3 wobble = new Vector3(Mathf.Sin(wobbleTimer), Mathf.Cos(wobbleTimer * 0.8f), 0f) * wobbleAmplitude;

        // --- Direction combinée ---
        Vector3 moveDir = (distance > 0 ? (targetPos - transform.position).normalized : Vector3.zero) + wobble * 0.01f;
        moveDir.Normalize();

        // --- Gestion de la vitesse ---
        float targetSpeed = speed;
        if (distance <= stopDistance)
            targetSpeed = Mathf.Lerp(speed, 0, (stopDistance - distance) / stopDistance);

        Vector3 desiredVelocity = moveDir * targetSpeed + avoidDir * speed;
        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, desiredVelocity, Time.fixedDeltaTime * smoothTurn);

        // --- Orientation fluide ---
        if (rb.linearVelocity.sqrMagnitude > 1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(rb.linearVelocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.fixedDeltaTime * smoothTurn);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Visualisation de la zone de détection
        Gizmos.color = avoiding ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRadius);
    }
}
