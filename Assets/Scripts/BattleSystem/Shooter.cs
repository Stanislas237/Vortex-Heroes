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
    public float aimError = 15f; // en degrés
    public float shootTimer = .1f;

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
        while (LoopCondition())
        {
            yield return new WaitForSeconds(shootTimer);

            if (!CanShoot() || !LoopCondition())
                continue;

            var firePoint = weaponHoles[index++ % weaponHoles.Count];
            Vector3 dirToPlayer = (target.position - firePoint.position).normalized;
            // Ajoute une imprécision aléatoire
            dirToPlayer = Quaternion.Euler(Random.Range(-aimError, aimError), Random.Range(-aimError, aimError), 0) * dirToPlayer;

            var laser = Instantiate(laserPrefab, firePoint.position, Quaternion.LookRotation(dirToPlayer));
            laser.AddComponent<Laser>().Initialize(gameObject.tag);

            // Modifier la couleur du laser
            objRenderer = laser.transform.GetChild(0).GetComponent<Renderer>();

            if (materialPropertyBlock != null)
            {
                materialPropertyBlock = new();
                objRenderer.GetPropertyBlock(materialPropertyBlock);
                materialPropertyBlock.SetColor("_Color", laserColor);
            }
            
            objRenderer?.SetPropertyBlock(materialPropertyBlock);
        }            
    }
}
