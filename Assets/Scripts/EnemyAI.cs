using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyAI : MonoBehaviour
{
    [HideInInspector]
    public Transform player;

    [Header("Movement")]
    public float baseSpeed = 60f;
    public float speedVariation = 10f;
    public float stopDistance = 100f;
    public float followDistance = 500f;
    public float smoothTurn = 3f;

    [Header("Agitation / Drift")]
    public float wobbleAmplitude = 1.5f;
    public float wobbleFrequency = 5f;

    [Header("Obstacle Avoidance")]
    public float detectRange = 80f;
    public float avoidStrength = 50f;
    public LayerMask obstacleMask;
    public float avoidCooldown = 1.5f;
    public float detectRadius = 30f;
    public Vector2 posThreshold;

    private Rigidbody rb;
    private float speed;
    private float wobbleTimer;
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

        // Vitesse initiale légèrement aléatoire
        speed = baseSpeed + Random.Range(-speedVariation, speedVariation);
    }

    void FixedUpdate()
    {
        if (!player) return;

        Vector3 targetPos = player.position + (Vector3)posThreshold + player.forward * stopDistance;
        float distance = (targetPos - transform.position).magnitude;

        // --- Obstacle detection ---
        avoiding = false;
        if (Time.time - lastAvoidTime > avoidCooldown)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, detectRadius, obstacleMask);

            if (hits.Length > 0)
            {
                Vector3 totalAvoid = Vector3.zero;

                foreach (var hit in hits)
                {
                    Vector3 awayFromObstacle = (transform.position - hit.transform.position).normalized;
                    totalAvoid += awayFromObstacle;
                }

                if (totalAvoid != Vector3.zero)
                {
                    totalAvoid.Normalize();

                    // On tourne légèrement à gauche ou droite (style dogfight)
                    float side = Random.value > 0.5f ? 90f : -90f;
                    avoidDir = (Quaternion.Euler(0, side, 0) * totalAvoid).normalized;
                    avoiding = true;
                    lastAvoidTime = Time.time;
                }
            }
        }

        // --- Drift naturel / agitation ---
        wobbleTimer += Time.fixedDeltaTime * wobbleFrequency;
        Vector3 wobble = new Vector3(Mathf.Sin(wobbleTimer), Mathf.Cos(wobbleTimer * 0.8f), 0f) * wobbleAmplitude;

        // --- Direction combinée ---
        Vector3 moveDir = (distance > 0 ? (targetPos - transform.position).normalized : Vector3.zero) + (avoiding ? avoidDir * 0.8f : Vector3.zero) + wobble * 0.01f;
        moveDir.Normalize();

        // --- Gestion de la vitesse ---
        float targetSpeed = speed;
        if (distance <= stopDistance)
            targetSpeed = Mathf.Lerp(speed, 0, (stopDistance - distance) / stopDistance);

        Vector3 desiredVelocity = moveDir * targetSpeed;
        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, desiredVelocity, Time.fixedDeltaTime * smoothTurn);

        // --- Orientation fluide ---
        if (rb.linearVelocity.sqrMagnitude > 1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(rb.linearVelocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.fixedDeltaTime * smoothTurn);
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.relativeVelocity.magnitude > 10f)
        {
            Destroy(gameObject);
            // Explosion à ajouter ici
        }
    }

    void OnDrawGizmosSelected()
    {
        // Visualisation de la zone de détection
        Gizmos.color = avoiding ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRadius);
    }
}
