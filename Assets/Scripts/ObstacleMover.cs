using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class ObstacleMover : MonoBehaviour
{
    public float speed = 800f;
    public float rotationSpeed = 50f;
    public Vector3 rotationAxis = Vector3.up;
    public Vector3 InitialDirection = Vector3.back;

    public static GameObject player;
    public static GameObject destroyEffect;
    public static int maxCollisionsToDestroy = 50;

    private Rigidbody rb;
    private Dictionary<string, int> collisionAmount = new();

    public void Init(Vector3 direction, float moveSpeed, Vector3 rotAxis, float rotSpeed)
    {
        InitialDirection = direction;
        speed = moveSpeed;
        rotationAxis = rotAxis;
        rotationSpeed = rotSpeed;
        rb ??= GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = false;
            rb.linearVelocity = InitialDirection.normalized * speed;
            rb.angularVelocity = rotationAxis.normalized * rotationSpeed * Mathf.Deg2Rad;
        }
        else
            Debug.LogWarning("No Rigidbody found on Obstacle. Using Transform for movement.");

        Invoke(nameof(DestroyAfterTimer), 60f); // nettoyage après passage
    }

    void StopAllMoves() => rb.linearVelocity = rb.angularVelocity = Vector3.zero;

    void DestroyAfterTimer()
    {
        if (player)
            Destroy(gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        var collisionHandler = collision.gameObject.GetComponentInParent<ShipCollisionHandler>();
        collisionHandler?.CollisionHandler(collision.contacts[0].point);
    }

    void OnCollisionStay(Collision collision)
    {
        var name = collision.gameObject.name;
        collisionAmount[name] = collisionAmount.GetValueOrDefault(name, 0) + 1;

        if (collisionAmount[name] >= maxCollisionsToDestroy)
        {
            if (collision.gameObject == player)
                foreach (var obstacle in ObstacleSpawner.obstacles)
                    obstacle?.StopAllMoves();
            
            if (destroyEffect != null)
            {
                Instantiate(destroyEffect, collision.gameObject.transform).transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                // rb.AddExplosionForce(500f, collision.contacts[0].point, 20f, 10f, ForceMode.Impulse);
                Destroy(collision.gameObject, .3f);
            }
        }
    }

    void OnDestroy() => ObstacleSpawner.obstacles.Remove(this);
    
    void OnDisable() => ObstacleSpawner.obstacles.Remove(this);
}
