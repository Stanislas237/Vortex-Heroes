using UnityEngine;

public class Laser : MonoBehaviour
{
    public float speed = 100f;
    public float lifeTime = 5f;
    public int damage = 5;
    public Vector3 direction;
    public GameObject hitEffect;

    public static GameObject lasersParent;

    private string ownerTag;

    public void Initialize(string ownerTag)
    {
        this.ownerTag = ownerTag;

        if (TryGetComponent(out Collider collider))
            collider.enabled = collider.isTrigger = true;

        Destroy(gameObject, lifeTime);

        if (lasersParent == null)
            lasersParent = GameObject.Find("LasersParent");
        else
            transform.parent.SetParent(lasersParent.transform);
    }

    void Update() => transform.Translate(-transform.forward * speed * Time.deltaTime);

    void OnTriggerEnter(Collider other)
    {
        var TopParent = other.transform.root;

        if (TopParent.CompareTag(ownerTag))
            return;
        Debug.Log($"Un Laser a touché le top object : {TopParent.name}");

        bool isObstacle = TopParent.CompareTag("Obstacle");
        bool isEnemy = TopParent.CompareTag("Enemy");
        bool isPlayer = TopParent.CompareTag("Player");

        if (!isObstacle && !isEnemy && !isPlayer)
            return;

        if (hitEffect && lasersParent)
        {
            // Trouver le point de contact le plus proche
            Vector3 hitPoint = other.ClosestPoint(transform.position);

            // Instancier l’effet d’impact orienté selon la normale
            GameObject fx = Instantiate(hitEffect, hitPoint, Quaternion.LookRotation(Vector3.up), lasersParent.transform);
            Destroy(fx, .3f);
        }

        if (TopParent.TryGetComponent(out Mortal m))
            m.TakeDamage(damage);
        Destroy(gameObject);
    }

    void OnDestroy() => Destroy(transform.parent.gameObject);
}
