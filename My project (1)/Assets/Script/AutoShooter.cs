

using UnityEngine;

public class AutoShooter : MonoBehaviour
{
    public GameObject bulletPrefab;

    [Header("攻撃設定")]
    public float fireInterval = 0.5f;
    public float attackRange = 10f;
    public int baseDamage = 10;

    private float nextFireTime;

    void Update()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.IsGameOver)
            return;

        if (bulletPrefab == null || Time.time < nextFireTime)
            return;

        Transform target = FindNearestTarget();

        if (target == null)
            return;

        Vector2 direction =
            ((Vector2)target.position -
             (Vector2)transform.position).normalized;

        GameObject bullet = Instantiate(
            bulletPrefab,
            transform.position,
            Quaternion.identity
        );

        BulletController controller =
            bullet.GetComponent<BulletController>();

        if (controller != null)
            controller.Launch(direction, baseDamage);

        nextFireTime = Time.time + fireInterval;
    }

    Transform FindNearestTarget()
    {
        Transform nearest = null;
        float nearestDistance = attackRange;

        EnemyController[] enemies =
            FindObjectsOfType<EnemyController>();

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

        BossController[] bosses =
            FindObjectsOfType<BossController>();

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

    public void UpgradeDamage()
    {
        baseDamage += 10;
        Debug.Log("攻撃力アップ！ 現在の攻撃力：" + baseDamage);
    }

    public void UpgradeAttackSpeed()
    {
        fireInterval = Mathf.Max(0.1f, fireInterval * 0.8f);
        Debug.Log("攻撃速度アップ！ 発射間隔：" + fireInterval);
    }
}

