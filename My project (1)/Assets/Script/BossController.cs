
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class BossController : MonoBehaviour
{
    [Header("ボスのHP")]
    public int maxHP = 500;
    public int currentHP;

    [Header("移動")]
    public float moveSpeed = 1.5f;
    public float secondPhaseSpeedMultiplier = 1.5f;

    [Header("接触攻撃")]
    public float attackRange = 2f;
    public int contactDamage = 25;
    public float attackInterval = 1.5f;

    [Header("突進攻撃")]
    public float dashWarningTime = 0.8f;
    public float dashSpeed = 10f;
    public float dashDuration = 0.45f;
    public float dashInterval = 5f;
    public int dashDamage = 35;

    [Header("弾幕攻撃")]
    public GameObject projectilePrefab;
    public float shootInterval = 4f;
    public float projectileSpeed = 4f;
    public int projectileDamage = 15;
    public int projectileCount = 5;
    public float spreadAngle = 60f;

    [Header("円形衝撃波")]
    public float shockwaveInterval = 7f;
    public float shockwaveWarningTime = 1f;
    public float shockwaveRadius = 3.5f;
    public int shockwaveDamage = 30;
    public float shockwaveVisualDuration = 0.4f;

    [Header("撃破報酬")]
    public int scoreValue = 500;
    public GameObject experiencePrefab;
    public int experienceValue = 10;

    [Header("見た目")]
    public Color normalColor = new Color(1f, 0.15f, 0.15f);
    public Color warningColor = new Color(1f, 0.85f, 0.1f);
    public Color phaseTwoColor = new Color(1f, 0f, 0.8f);

    private Rigidbody2D rb;
    private PlayerController player;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private float nextAttackTime;
    private float nextDashTime;
    private float nextShootTime;
    private float nextShockwaveTime;

    private bool isDead;
    private bool isDashing;
    private bool secondPhaseStarted;

    private LineRenderer shockwaveLine;
    private Coroutine shockwaveCoroutine;

    public Color shockwaveWarningColor = new Color(1f, 0.05f, 0.05f, 0.8f);

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
        spriteRenderer = GetComponent<SpriteRenderer>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        currentHP = maxHP;

        if (spriteRenderer != null)
        {
            originalColor = normalColor;
            spriteRenderer.color = normalColor;
        }

        CreateShockwaveVisual();
    }

    void Start()
    {
        player = FindFirstObjectByType<PlayerController>();

        if (player == null)
            Debug.LogWarning("BossController: Playerが見つかりません。");

        // 最初の攻撃まで少し時間を空ける
        nextAttackTime = Time.time + 2f;
        nextDashTime = Time.time + 4f;
        nextShootTime = Time.time + 3f;
        nextShockwaveTime = Time.time + 6f;
    }

    void FixedUpdate()
    {
        if (isDead || player == null || player.currentHP <= 0)
            return;

        if (isDashing)
            return;

        float currentSpeed = secondPhaseStarted
            ? moveSpeed * secondPhaseSpeedMultiplier
            : moveSpeed;

        Vector2 toPlayer =
            (Vector2)player.transform.position - rb.position;

        float distance = toPlayer.magnitude;

        // 近づきすぎないように追跡
        if (distance > attackRange * 0.8f)
        {
            Vector2 direction = toPlayer.normalized;

            rb.MovePosition(
                rb.position +
                direction * currentSpeed * Time.fixedDeltaTime
            );
        }

        // 接触攻撃
        if (distance <= attackRange &&
            Time.time >= nextAttackTime)
        {
            player.TakeDamage(contactDamage);
            nextAttackTime = Time.time + attackInterval;
        }
    }

    void Update()
    {
        if (isDead || player == null || player.currentHP <= 0)
            return;

        // ゲームオーバー後は攻撃しない
        if (GameManager.Instance != null &&
            GameManager.Instance.IsGameOver)
            return;

        // HP半分以下で第2形態
        if (!secondPhaseStarted && HPPercent <= 0.5f)
        {
            secondPhaseStarted = true;

            if (spriteRenderer != null)
                spriteRenderer.color = phaseTwoColor;

            // 第2形態突入時に攻撃を少し早める
            nextDashTime = Mathf.Min(nextDashTime, Time.time + 1f);
            nextShootTime = Mathf.Min(nextShootTime, Time.time + 1f);

            Debug.Log("BOSS SECOND PHASE!");
        }

        if (isDashing)
            return;

        // 優先順位：衝撃波 → 突進 → 弾幕
        if (Time.time >= nextShockwaveTime)
        {
            nextShockwaveTime = Time.time +
                (secondPhaseStarted
                    ? shockwaveInterval * 0.7f
                    : shockwaveInterval);

            shockwaveCoroutine = StartCoroutine(ShockwaveAttack());
        }
        else if (Time.time >= nextDashTime)
        {
            nextDashTime = Time.time +
                (secondPhaseStarted
                    ? dashInterval * 0.65f
                    : dashInterval);

            StartCoroutine(DashAttack());
        }
        else if (Time.time >= nextShootTime)
        {
            nextShootTime = Time.time +
                (secondPhaseStarted
                    ? shootInterval * 0.65f
                    : shootInterval);

            ShootFan();
        }
    }

    IEnumerator DashAttack()
    {
        if (isDead || player == null)
            yield break;

        isDashing = true;

        // 黄色に変化して突進を予告
        SetBossColor(warningColor);

        yield return new WaitForSeconds(dashWarningTime);

        if (isDead || player == null)
        {
            isDashing = false;
            yield break;
        }

        // 予告終了時のプレイヤー位置に向かって突進
        Vector2 direction =
            ((Vector2)player.transform.position - rb.position).normalized;

        float elapsed = 0f;
        bool hitPlayer = false;

        SetBossColor(secondPhaseStarted
            ? phaseTwoColor
            : Color.white);

        while (elapsed < dashDuration && !isDead)
        {
            Vector2 nextPosition =
                rb.position + direction * dashSpeed *
                (secondPhaseStarted ? 1.3f : 1f) *
                Time.fixedDeltaTime;

            rb.MovePosition(nextPosition);

            // 突進中の接触を検出
            if (!hitPlayer && player != null &&
                player.currentHP > 0)
            {
                float hitDistance = Vector2.Distance(
                    nextPosition,
                    player.transform.position
                );

                if (hitDistance < 1.1f)
                {
                    player.TakeDamage(
                        secondPhaseStarted
                            ? Mathf.RoundToInt(dashDamage * 1.3f)
                            : dashDamage
                    );

                    hitPlayer = true;
                }
            }

            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        isDashing = false;

        SetBossColor(secondPhaseStarted
            ? phaseTwoColor
            : normalColor);
    }

    void ShootFan()
    {
        if (projectilePrefab == null || player == null)
            return;

        Vector2 direction =
            ((Vector2)player.transform.position - rb.position).normalized;

        int count = secondPhaseStarted
            ? projectileCount + 2
            : projectileCount;

        float totalAngle = secondPhaseStarted
            ? spreadAngle + 20f
            : spreadAngle;

        for (int i = 0; i < count; i++)
        {
            float angle = count <= 1
                ? 0f
                : -totalAngle / 2f +
                  totalAngle * i / (count - 1);

            Vector2 shotDirection =
                Quaternion.Euler(0f, 0f, angle) * direction;

            GameObject projectile = Instantiate(
                projectilePrefab,
                transform.position,
                Quaternion.identity
            );

            EnemyProjectile controller =
                projectile.GetComponent<EnemyProjectile>();

            if (controller != null)
            {
                controller.Launch(
                    shotDirection,
                    projectileSpeed * (secondPhaseStarted ? 1.2f : 1f),
                    projectileDamage
                );
            }
            else
            {
                Debug.LogError(
                    "ボスの弾PrefabにEnemyProjectileがありません！"
                );

                Destroy(projectile);
            }
        }
    }

    IEnumerator ShockwaveAttack()
    {
        if (isDead)
            yield break;

        SetBossColor(warningColor);

        // 攻撃する位置を決める
        Vector3 center = transform.position;

        // 赤い円で危険範囲を予告
        if (shockwaveLine != null)
        {
            shockwaveLine.transform.position = center;
            shockwaveLine.startColor = shockwaveWarningColor;
            shockwaveLine.endColor = shockwaveWarningColor;
            shockwaveLine.startWidth = 0.12f;
            shockwaveLine.endWidth = 0.12f;

            DrawShockwaveCircle(shockwaveRadius);
            shockwaveLine.enabled = true;
        }

        // 予告中はプレイヤーが回避できる
        yield return new WaitForSeconds(shockwaveWarningTime);

        if (isDead)
            yield break;

        SetBossColor(secondPhaseStarted
            ? phaseTwoColor
            : normalColor);

        // 衝撃波が外側へ広がる
        float elapsed = 0f;

        while (elapsed < shockwaveVisualDuration)
        {
            if (shockwaveLine != null)
            {
                float radius = Mathf.Lerp(
                    0.2f,
                    shockwaveRadius,
                    elapsed / shockwaveVisualDuration
                );

                DrawShockwaveCircle(radius);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 衝撃波の範囲内にいるプレイヤーへダメージ
        if (!isDead && player != null && player.currentHP > 0)
        {
            float distance = Vector2.Distance(
                center,
                player.transform.position
            );

            // プレイヤーの大きさも考慮して判定
            float hitRadius = shockwaveRadius + 0.5f;

            Debug.Log(
                $"衝撃波判定：距離={distance:F2} / 範囲={hitRadius:F2}"
            );

            if (distance <= hitRadius)
            {
                int damage = secondPhaseStarted
                    ? Mathf.RoundToInt(shockwaveDamage * 1.3f)
                    : shockwaveDamage;

                player.TakeDamage(damage);

                Debug.Log($"衝撃波命中！ {damage} ダメージ");
            }
            else
            {
                Debug.Log("衝撃波回避！");
            }
        }

        if (shockwaveLine != null)
        {
            shockwaveLine.enabled = false;
        }
    }

    void CreateShockwaveVisual()
    {
        GameObject visual = new GameObject("BossShockwaveVisual");
        visual.transform.SetParent(transform, false);
        visual.transform.localPosition = Vector3.zero;

        shockwaveLine = visual.AddComponent<LineRenderer>();
        shockwaveLine.useWorldSpace = false;
        shockwaveLine.loop = true;
        shockwaveLine.positionCount = 49;
        shockwaveLine.startWidth = 0.12f;
        shockwaveLine.endWidth = 0.12f;
        shockwaveLine.sortingOrder = 30;
        shockwaveLine.enabled = false;

        Shader shader = Shader.Find("Sprites/Default");

        if (shader != null)
            shockwaveLine.material = new Material(shader);

        shockwaveLine.startColor = Color.cyan;
        shockwaveLine.endColor = Color.cyan;
    }

    void DrawShockwaveCircle(float radius)
    {
        if (shockwaveLine == null)
            return;

        for (int i = 0; i < shockwaveLine.positionCount; i++)
        {
            float angle = i * Mathf.PI * 2f /
                          shockwaveLine.positionCount;

            shockwaveLine.SetPosition(
                i,
                new Vector3(
                    Mathf.Cos(angle) * radius,
                    Mathf.Sin(angle) * radius,
                    0f
                )
            );
        }
    }

    void SetBossColor(Color color)
    {
        if (spriteRenderer != null)
            spriteRenderer.color = color;
    }

    public void TakeDamage(int damage)
    {
        if (isDead || damage <= 0)
            return;

        currentHP = Mathf.Max(0, currentHP - damage);

        if (!secondPhaseStarted && HPPercent <= 0.5f)
        {
            // Updateで第2形態へ切り替える
        }

        if (currentHP <= 0)
            Die();
    }

    void Die()
    {
        if (isDead)
            return;

        isDead = true;

        StopAllCoroutines();

        if (shockwaveLine != null)
            shockwaveLine.enabled = false;

        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(scoreValue);

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

        Debug.Log("BOSS DEFEATED!");

        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (shockwaveLine != null && shockwaveLine.material != null)
            Destroy(shockwaveLine.material);
    }
}



