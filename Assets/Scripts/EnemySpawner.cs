using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public Transform player;
    public GameObject[] enemyPrefabs;
    public float spawnDistance = 400f;
    public Vector2 spawnInterval = new(3f, 6f);
    public Vector2 spawnRangeX = new(-150f, 150f);
    public Vector2 spawnRangeY = new(-50f, 50f);
    public LayerMask obstacleMask;


    private float timer;

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            SpawnEnemy();
            timer = Random.Range(spawnInterval.x, spawnInterval.y);
        }
    }

    void SpawnEnemy()
    {
        if (!player) return;

        // Position random autour de la ligne avant du joueur
        Vector3 spawnThresold = new(Random.Range(spawnRangeX.x, spawnRangeX.y), Random.Range(spawnRangeY.x, spawnRangeY.y), spawnDistance);

        GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        Instantiate(prefab, player.position + spawnThresold, Quaternion.identity).AddComponent<EnemyAI>().Init(player, (Vector2)spawnThresold, obstacleMask);
    }
}
