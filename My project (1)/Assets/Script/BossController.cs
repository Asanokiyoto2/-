
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BossController : MonoBehaviour
{
    [Header("ボスのHP")]
    public int maxHP = 500;
    public int currentHP;

    [Header("移動")]
    public float moveSpeed = 1.5f;
    public float secondPhaseSpeedMultiplier = 1.5f;

    [Header("攻撃")]
    public float attackRange = 2f;
    public int contactDamage = 25;
    public float attackInterval = 1.5f;

    [Header("撃破報酬")]
    public int scoreValue = 500;
    public GameObject experiencePrefab;
    public int experienceValue = 10;

    private Rigidbody2D rb;
    private PlayerController player;
    private float nextAttackTime;
    private bool isDead;

    public float HPPercent
    {
        get
        {
            if (maxHP <= 0) return 0f;
            return (float)currentHP / maxHP;
        }
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        currentHP = maxHP;
    }

    void Start()
    {
        player = FindFirstObjectByType<PlayerController>();

        if (player == null)
        {
            Debug.LogWarning("BossController: Playerが見つかりません。");
        }
    }

    void FixedUpdate()
    {
        if (isDead || player == null || player.currentHP <= 0)
            return;

        Vector2 toPlayer =
            (Vector2)player.transform.position - rb.position;

        float distance = toPlayer.magnitude;

        // HPが半分以下になると第2形態
        float currentSpeed = HPPercent <= 0.5f
            ? moveSpeed * secondPhaseSpeedMultiplier
            : moveSpeed;

        // プレイヤーに近づきすぎたらその場で攻撃
        if (distance > attackRange * 0.8f)
        {
            Vector2 direction = toPlayer.normalized;

            rb.MovePosition(
                rb.position +
                direction * currentSpeed * Time.fixedDeltaTime
            );
        }

        // 一定時間ごとに接近攻撃
        if (distance <= attackRange && Time.time >= nextAttackTime)
        {
            player.TakeDamage(contactDamage);
            nextAttackTime = Time.time + attackInterval;
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead || damage <= 0) return;

        currentHP -= damage;
        currentHP = Mathf.Max(0, currentHP);

        if (HPPercent <= 0.5f)
        {
            // 第2形態になったとき一度だけログを出す
            if (!secondPhaseStarted)
            {
                secondPhaseStarted = true;
                Debug.Log("BOSS SECOND PHASE!");
            }
        }

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private bool secondPhaseStarted = false;

    void Die()
    {
        if (isDead) return;

        isDead = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue);
        }

        if (experiencePrefab != null)
        {
            GameObject orb = Instantiate(
                experiencePrefab,
                transform.position,
                Quaternion.identity
            );

            ExperienceOrb xp = orb.GetComponent<ExperienceOrb>();

            if (xp != null)
            {
                xp.value = experienceValue;
            }
        }

        Debug.Log("BOSS DEFEATED!");

        Destroy(gameObject);
    }
}


