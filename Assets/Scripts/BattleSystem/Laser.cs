using UnityEngine;

public class Laser : MonoBehaviour
{
    public float speed = 400f;
    public float lifeTime = 5f;
    public int damage = 25;
    public bool canDamageObstacles = false;
    public GameObject hitEffect;

    private string ownerTag;

    public void Initialize(string owner)
    {
        ownerTag = owner;
        Destroy(gameObject, lifeTime);
    }

    void Update() => transform.Translate(Vector3.forward * speed * Time.deltaTime);

    void OnTriggerEnter(Collider other)
    {
        var TopParent = Utils.GetTopMostParent(other.transform);
        if (TopParent.CompareTag(ownerTag)) return; // ignore tir ami

        bool isObstacle = TopParent.CompareTag("Obstacle");
        bool isEnemy = TopParent.CompareTag("Enemy");
        bool isPlayer = TopParent.CompareTag("Player");

        if ((isObstacle && !canDamageObstacles) || (!isObstacle && !isEnemy && !isPlayer))
            return;

        if (hitEffect)
        {
            // Trouver le point de contact le plus proche
            Vector3 hitPoint = other.ClosestPoint(transform.position);

            // Calculer une approximation de la normale (direction opposée du laser)
            Vector3 hitNormal = (hitPoint - transform.position).normalized;

            // Instancier l’effet d’impact orienté selon la normale
            GameObject fx = Instantiate(hitEffect, hitPoint, Quaternion.LookRotation(hitNormal));
            Destroy(fx, .5f);
        }

        if (TopParent.TryGetComponent(out Mortal m))
            m.TakeDamage(damage);            
        Destroy(gameObject);
    }
}
