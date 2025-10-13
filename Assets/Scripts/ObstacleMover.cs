using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ObstacleMover : MonoBehaviour
{
    public float speed = 800f;
    public float rotationSpeed = 50f;
    public Vector3 rotationAxis = Vector3.up;
    public Vector3 InitialDirection = Vector3.back;
    private Transform player;
    private Rigidbody rb;

    public void Init(Vector3 direction, float moveSpeed, Vector3 rotAxis, float rotSpeed)
    {
        InitialDirection = direction;
        speed = moveSpeed;
        rotationAxis = rotAxis;
        rotationSpeed = rotSpeed;
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = false;
            rb.linearVelocity = InitialDirection.normalized * speed;
            rb.angularVelocity = rotationAxis.normalized * rotationSpeed * Mathf.Deg2Rad;
        }
        else
            Debug.LogWarning("No Rigidbody found on Obstacle. Using Transform for movement.");

        Destroy(gameObject, 10f); // nettoyage après passage
    }

    void ApplyTagRecursively(Transform obj, string tag)
    {
        obj.tag = tag;
        foreach (Transform child in obj)
            ApplyTagRecursively(child, tag);
    }
}
