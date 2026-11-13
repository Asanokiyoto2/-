

using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("ìGPrefab")]
    public GameObject enemyPrefab;
    public GameObject rangedEnemyPrefab;
    public GameObject bossPrefab;

    [Header("èoåªê›íË")]
    public float spawnInterval = 2f;
    public float spawnRadius = 8f;
    public int maxEnemies = 30;
    [Range(0f, 1f)]
    public float rangedSpawnChance = 0.25f;

    [Header("É{ÉXê›íË")]
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
        if (player == null || player.currentHP <= 0)
            return;

        if (!bossSpawned &&
            bossPrefab != null &&
            Time.time - startTime >= bossSpawnTime)
        {
            SpawnBoss();
        }

        if (enemyPrefab == null || Time.time < nextSpawnTime)
            return;

        if (FindObjectsByType<EnemyController>(FindObjectsSortMode.None).Length < maxEnemies)
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
        GameObject prefabToSpawn = enemyPrefab;

        if (rangedEnemyPrefab != null &&
            Random.value < rangedSpawnChance)
        {
            prefabToSpawn = rangedEnemyPrefab;
        }

        Instantiate(
            prefabToSpawn,
            GetSpawnPosition(),
            Quaternion.identity
        );
    }

    void SpawnBoss()
    {
        Instantiate(
            bossPrefab,
            GetSpawnPosition(),
            Quaternion.identity
        );

        bossSpawned = true;
    }
}


