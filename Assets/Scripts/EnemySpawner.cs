using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public static List<EnemyAI> enemies;

    [Header("Target (the player or ship)")]
    public Transform player;

    [Header("Enemies")]
    public GameObject[] enemyPrefabs;
    public GameObject laserPrefab;

    [Header("Spawn Settings")]
    public float spawnDistance = 400f;
    public Vector2 spawnRangeX = new(-150f, 150f);
    public Vector2 spawnRangeY = new(-50f, 50f);
    public Vector2 spawnInterval = new(3f, 6f);

    [Header("Obstacle detection")]
    public LayerMask obstacleMask;


    void Start() => StartCoroutine(SpawnRoutine());

    IEnumerator SpawnRoutine()
    {
        enemies = new();

        while (player != null)
        {
            if (!player) yield break;

            // Position random autour de la ligne avant du joueur
            Vector3 spawnThresold = new(Random.Range(spawnRangeX.x, spawnRangeX.y), Random.Range(spawnRangeY.x, spawnRangeY.y), spawnDistance);

            var enemyObj = Instantiate(enemyPrefabs[Random.Range(0, enemyPrefabs.Length)], player.position + spawnThresold, Quaternion.identity);
            var enemy = enemyObj.AddComponent<EnemyAI>();
            enemy.Init(player, (Vector2)spawnThresold, obstacleMask);
            enemies.Add(enemy);

            var enemyShooter = enemyObj.AddComponent<Shooter>();
            enemyShooter.target = player;
            enemyShooter.Init(Color.red, 7f, .2f);
            enemyShooter.laserPrefab = laserPrefab;

            yield return new WaitForSeconds(Random.Range(spawnInterval.x, spawnInterval.y));
        }
    }
}
