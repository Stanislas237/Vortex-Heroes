using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class ObstacleMover : MonoBehaviour
{
    public float speed = 800f;
    public float rotationSpeed = 50f;
    public Vector3 rotationAxis = Vector3.up;
    public Vector3 InitialDirection = Vector3.back;
    public static GameObject destroyEffect;
    public static int maxCollisionsToDestroy = 50;

    private Rigidbody rb;
    private static GameObject player;
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

        Destroy(gameObject, 60f); // nettoyage après passage
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
            Debug.Log("Collision Stay with " + name);
            
            if (destroyEffect != null)
            {
                Instantiate(destroyEffect, collision.gameObject.transform).transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                rb.AddExplosionForce(500f, collision.contacts[0].point, 20f, 10f, ForceMode.Impulse);
                Destroy(collision.gameObject, 2.3f);
            }
        }
    }

    // void ApplyTagRecursively(Transform obj, string tag)
    // {
    //     obj.tag = tag;
    //     foreach (Transform child in obj)
    //         ApplyTagRecursively(child, tag);
    // }
}
