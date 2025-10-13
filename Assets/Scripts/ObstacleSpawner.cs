using UnityEngine;
using System.Collections;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Target (the player or ship)")]
    public Transform player;

    [Header("Obstacles")]
    public GameObject[] obstaclePrefabs;

    [Header("Spawn Settings")]
    public float spawnDistance = 200f;
    public Vector2 spawnRangeX = new(-150f, 150f);
    public Vector2 spawnRangeY = new(-50f, 50f);
    public Vector2 spawnInterval = new(1.5f, 3.5f);

    [Header("Scale Variation")]
    public Vector2 scaleRange = new(0.6f, 2.5f);

    void Start() => StartCoroutine(SpawnRoutine());

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(spawnInterval.x, spawnInterval.y));

            if (Random.value >= 0.65f) continue; // 35% de chance de spawn

            // Choix aléatoire d’un prefab
            GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];

            // Position de spawn devant le joueur
            Vector3 spawnPos = player.position + player.forward * spawnDistance;
            spawnPos += new Vector3(Random.Range(spawnRangeX.x, spawnRangeX.y), Random.Range(spawnRangeY.x, spawnRangeY.y), 0);

            // Création de l’obstacle
            GameObject obj = Instantiate(prefab, spawnPos, Random.rotation);
            obj.transform.localScale *= Random.Range(scaleRange.x, scaleRange.y);
        }
    }
}
