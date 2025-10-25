using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Shooter : MonoBehaviour
{
    [HideInInspector]
    public List<Transform> weaponHoles;
    [HideInInspector]
    public Transform target;
    public GameObject laserPrefab;

    [SerializeField]
    private string holesTag = "LaserHole";

    [Header("Shooter settings")]
    public float aimError = .8f; // en degrés
    public float shootTimer = .2f;

    private Renderer objRenderer;
    private Color laserColor;
    private MaterialPropertyBlock materialPropertyBlock;

    public virtual void Init(Color laserColor, float aimError, float shootTimer)
    {
        weaponHoles = Utils.GetTransformsTag(transform, holesTag);
        this.aimError = aimError;
        this.shootTimer = shootTimer;
        this.laserColor = laserColor;

        StartCoroutine(Shoot());
    }

    protected virtual bool CanShoot() => true;
    
    protected virtual bool LoopCondition() => target && weaponHoles.Count > 0;

    private IEnumerator Shoot()
    {
        int index = 0;

        yield return new WaitForSeconds(shootTimer);
        
        // On vérifie une seule fois si la boucle peut commencer.
        while (LoopCondition())
        {
            // Si on ne peut pas tirer, ou si la boucle n'est plus valide, on passe à l'itération suivante.
            if (CanShoot())
            {                
                // Calcul du point de tir et de la direction vers la cible
                var firePoint = weaponHoles[index++ % weaponHoles.Count];
                Vector3 dirToPlayer = (target.position - firePoint.position).normalized;

                // Appliquer une imprécision aléatoire à la direction du tir
                dirToPlayer = ApplyAimError(dirToPlayer);

                // Instancier le laser
                var laser = InstantiateLaser(firePoint.position, dirToPlayer);

                // Modifier la couleur du laser si nécessaire
                SetLaserColor(laser);
            }

            // Attendre entre chaque tir.
            yield return new WaitForSeconds(shootTimer);
        }
    }

    // Applique l'imprécision à la direction du tir
    private Vector3 ApplyAimError(Vector3 direction) =>
        // Génère un écart aléatoire pour la direction de tir
        Quaternion.Euler(Random.Range(-aimError, aimError), Random.Range(-aimError, aimError), 0) * direction;

    // Crée et instancie le laser
    private Transform InstantiateLaser(Vector3 position, Vector3 direction)
    {
        var laser = Instantiate(laserPrefab, position, Quaternion.LookRotation(direction));
        laser.GetComponentInChildren<Laser>().Initialize(gameObject.tag, gameObject.layer); // Initialise le laser avec le tag de l'objet
        return laser.transform;
    }

    // Applique la couleur au laser instancié
    private void SetLaserColor(Transform laser)
    {
        var objRenderer = laser.GetChild(0).GetComponent<Renderer>();

        // On ne crée le PropertyBlock qu'une seule fois
        if (materialPropertyBlock == null)
        {
            materialPropertyBlock = new();
            objRenderer.GetPropertyBlock(materialPropertyBlock);  // Récupère l'état actuel du matériau
            materialPropertyBlock.SetColor("_Color", laserColor); // Applique la nouvelle couleur            
        }

        objRenderer.SetPropertyBlock(materialPropertyBlock);   // Applique les modifications au matériau
    }
}
