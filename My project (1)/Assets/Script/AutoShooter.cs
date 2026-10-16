
using UnityEngine;

public class AutoShooter : MonoBehaviour
{
    public GameObject bulletPrefab;
    public float fireInterval = 0.5f;
    public float attackRange = 10f;
    public int baseDamage = 10;

    private float nextFireTime;
    private LevelSystem levelSystem;

    void Start()
    {
        levelSystem = GetComponent<LevelSystem>();
    }

    void Update()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.IsGameOver) return;

        if (bulletPrefab == null || Time.time < nextFireTime) return;

        Transform target = FindNearestTarget();

        if (target == null) return;

        Vector2 direction =
            ((Vector2)target.position - (Vector2)transform.position).normalized;

        GameObject bullet = Instantiate(
            bulletPrefab, transform.position, Quaternion.identity);

        BulletController controller =
            bullet.GetComponent<BulletController>();

        if (controller != null)
        {
            int levelBonus = levelSystem != null
                ? (levelSystem.Level - 1) * 2
                : 0;

            controller.Launch(direction, baseDamage + levelBonus);
        }

        nextFireTime = Time.time + fireInterval;
    }

    Transform FindNearestTarget()
    {
        Transform nearest = null;
        float nearestDistance = attackRange;

        EnemyController[] enemies = FindObjectsOfType<EnemyController>();

        foreach (EnemyController enemy in enemies)
        {
            float distance = Vector2.Distance(
                transform.position, enemy.transform.position);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = enemy.transform;
            }
        }

        BossController[] bosses = FindObjectsOfType<BossController>();

        foreach (BossController boss in bosses)
        {
            float distance = Vector2.Distance(
                transform.position, boss.transform.position);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = boss.transform;
            }
        }

        return nearest;
    }
}

