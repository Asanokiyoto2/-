
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public GameObject bossPrefab;

    public float spawnInterval = 2f;
    public float spawnRadius = 8f;
    public int maxEnemies = 30;
    public float bossSpawnTime = 60f;

    private PlayerController player;
    private float nextSpawnTime;
    private float startTime;
    private bool bossSpawned;

    void Start()
    {
        player = FindFirstObjectByType<PlayerController>();
        startTime = Time.time;
    }

    void Update()
    {
        if (player == null || player.currentHP <= 0) return;

        if (!bossSpawned &&
            bossPrefab != null &&
            Time.time - startTime >= bossSpawnTime)
        {
            SpawnBoss();
        }

        if (enemyPrefab == null || Time.time < nextSpawnTime) return;

        if (FindObjectsOfType<EnemyController>().Length < maxEnemies)
        {
            SpawnEnemy();
        }

        nextSpawnTime = Time.time + spawnInterval;
    }

    Vector3 GetSpawnPosition()
    {
        Vector2 direction = Random.insideUnitCircle.normalized;

        if (direction == Vector2.zero)
            direction = Vector2.right;

        return player.transform.position +
               (Vector3)(direction * spawnRadius);
    }

    void SpawnEnemy()
    {
        Instantiate(enemyPrefab, GetSpawnPosition(), Quaternion.identity);
    }

    void SpawnBoss()
    {
        Instantiate(bossPrefab, GetSpawnPosition(), Quaternion.identity);
        bossSpawned = true;
    }
}

