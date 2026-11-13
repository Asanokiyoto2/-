
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    public enum EnemyType
    {
        Melee,
        Ranged
    }

    [Header("敵の種類")]
    public EnemyType enemyType = EnemyType.Melee;

    [Header("基本設定")]
    public float moveSpeed = 2f;
    public int maxHP = 30;
    public int contactDamage = 10;
    public int scoreValue = 10;
    public int experienceValue = 1;
    public float damageInterval = 1f;
    public GameObject experiencePrefab;

    [Header("遠距離攻撃設定")]
    public GameObject enemyProjectilePrefab;
    public float preferredDistance = 5f;
    public float distanceTolerance = 0.8f;
    public float fireInterval = 2f;
    public float projectileSpeed = 5f;
    public int projectileDamage = 10;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Coroutine hitFlashCoroutine;

    private int currentHP;
    private Rigidbody2D rb;
    private PlayerController player;
    private float nextDamageTime;
    private float nextFireTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        currentHP = maxHP;
    }

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        player = FindFirstObjectByType<PlayerController>();
        nextFireTime = Time.time + Random.Range(0f, fireInterval);
    }

    void Update()
    {
        if (player == null || player.currentHP <= 0)
            return;

        if (enemyType == EnemyType.Ranged)
        {
            TryShoot();
        }
    }

    void FixedUpdate()
    {
        if (player == null || player.currentHP <= 0)
            return;

        Vector2 toPlayer =
            (Vector2)player.transform.position - rb.position;

        float distance = toPlayer.magnitude;

        if (distance <= 0.01f)
            return;

        Vector2 direction = toPlayer.normalized;

        if (enemyType == EnemyType.Melee)
        {
            // 通常敵：プレイヤーに近づく
            rb.MovePosition(
                rb.position + direction * moveSpeed * Time.fixedDeltaTime
            );
        }
        else
        {
            // 遠距離敵：プレイヤーとの距離を保つ
            if (distance > preferredDistance + distanceTolerance)
            {
                rb.MovePosition(
                    rb.position + direction * moveSpeed * Time.fixedDeltaTime
                );
            }
            else if (distance < preferredDistance - distanceTolerance)
            {
                rb.MovePosition(
                    rb.position - direction * moveSpeed * Time.fixedDeltaTime
                );
            }
        }
    }

    void TryShoot()
    {
        if (enemyProjectilePrefab == null)
            return;

        if (Time.time < nextFireTime)
            return;

        Vector2 direction =
            ((Vector2)player.transform.position - rb.position).normalized;

        GameObject projectile = Instantiate(
            enemyProjectilePrefab,
            transform.position,
            Quaternion.identity
        );

        EnemyProjectile projectileScript =
            projectile.GetComponent<EnemyProjectile>();

        if (projectileScript != null)
        {
            projectileScript.Launch(
                direction,
                projectileSpeed,
                projectileDamage
            );
        }

        nextFireTime = Time.time + Mathf.Max(0.1f, fireInterval);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        PlayerController target =
            other.GetComponentInParent<PlayerController>();

        if (target != null && Time.time >= nextDamageTime)
        {
            target.TakeDamage(contactDamage);
            nextDamageTime = Time.time + damageInterval;
        }
    }

    public void TakeDamage(int damage)
    {
        if (currentHP <= 0)
            return;

        currentHP -= damage;
        PlayHitFlash();

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (experiencePrefab != null)
        {
            GameObject orb = Instantiate(
                experiencePrefab,
                transform.position,
                Quaternion.identity
            );

            ExperienceOrb xp = orb.GetComponent<ExperienceOrb>();

            if (xp != null)
                xp.value = experienceValue;
        }

        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(scoreValue);

        if (spriteRenderer != null)
        {
            DeathEffect.Play(
                spriteRenderer.sprite,
                transform.position,
                spriteRenderer.color
            );
        }

        Destroy(gameObject);
    }

    public void PlayHitFlash()
    {
        if (spriteRenderer == null)
            return;

        if (hitFlashCoroutine != null)
            StopCoroutine(hitFlashCoroutine);

        hitFlashCoroutine = StartCoroutine(HitFlash());
    }

    IEnumerator HitFlash()
    {
        spriteRenderer.color = Color.white;

        yield return new WaitForSeconds(0.08f);

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        hitFlashCoroutine = null;
    }
}


