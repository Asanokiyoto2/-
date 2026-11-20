
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class BossController : MonoBehaviour
{
    [Header("É{ÉXÇÃHP")]
    public int maxHP = 500;
    public int currentHP;

    [Header("à⁄ìÆ")]
    public float moveSpeed = 1.5f;
    public float secondPhaseSpeedMultiplier = 1.5f;

    [Header("ê⁄êGçUåÇ")]
    public float attackRange = 2f;
    public int contactDamage = 25;
    public float attackInterval = 1.5f;

    [Header("ìÀêiçUåÇ")]
    public float dashWarningTime = 0.8f;
    public float dashSpeed = 10f;
    public float dashDuration = 0.45f;
    public float dashInterval = 5f;
    public int dashDamage = 35;

    [Header("íeñãçUåÇ")]
    public GameObject projectilePrefab;
    public float shootInterval = 4f;
    public float projectileSpeed = 4f;
    public int projectileDamage = 15;
    public int projectileCount = 5;
    public float spreadAngle = 60f;

    [Header("â~å`è’åÇîg")]
    public float shockwaveInterval = 7f;
    public float shockwaveWarningTime = 1f;
    public float shockwaveRadius = 3.5f;
    public int shockwaveDamage = 30;
    public float shockwaveVisualDuration = 0.4f;

    [Header("åÇîjïÒèV")]
    public int scoreValue = 500;
    public GameObject experiencePrefab;
    public int experienceValue = 10;

    [Header("å©ÇΩñ⁄")]
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
            Debug.LogWarning("BossController: PlayerÇ™å©Ç¬Ç©ÇËÇ‹ÇπÇÒÅB");

        // ç≈èâÇÃçUåÇÇ‹Ç≈è≠Çµéûä‘ÇãÛÇØÇÈ
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

        // ãﬂÇ√Ç´Ç∑Ç¨Ç»Ç¢ÇÊÇ§Ç…í«ê’
        if (distance > attackRange * 0.8f)
        {
            Vector2 direction = toPlayer.normalized;

            rb.MovePosition(
                rb.position +
                direction * currentSpeed * Time.fixedDeltaTime
            );
        }

        // ê⁄êGçUåÇ
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

        // ÉQÅ[ÉÄÉIÅ[ÉoÅ[å„ÇÕçUåÇÇµÇ»Ç¢
        if (GameManager.Instance != null &&
            GameManager.Instance.IsGameOver)
            return;

        // HPîºï™à»â∫Ç≈ëÊ2å`ë‘
        if (!secondPhaseStarted && HPPercent <= 0.5f)
        {
            secondPhaseStarted = true;

            if (spriteRenderer != null)
                spriteRenderer.color = phaseTwoColor;

            // ëÊ2å`ë‘ìÀì¸éûÇ…çUåÇÇè≠ÇµëÅÇﬂÇÈ
            nextDashTime = Mathf.Min(nextDashTime, Time.time + 1f);
            nextShootTime = Mathf.Min(nextShootTime, Time.time + 1f);

            Debug.Log("BOSS SECOND PHASE!");
        }

        if (isDashing)
            return;

        // óDêÊèáà ÅFè’åÇîg Å® ìÀêi Å® íeñã
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

        // â©êFÇ…ïœâªÇµÇƒìÀêiÇó\çê
        SetBossColor(warningColor);

        yield return new WaitForSeconds(dashWarningTime);

        if (isDead || player == null)
        {
            isDashing = false;
            yield break;
        }

        // ó\çêèIóπéûÇÃÉvÉåÉCÉÑÅ[à íuÇ…å¸Ç©Ç¡ÇƒìÀêi
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

            // ìÀêiíÜÇÃê⁄êGÇåüèo
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
                    "É{ÉXÇÃíePrefabÇ…EnemyProjectileÇ™Ç†ÇËÇ‹ÇπÇÒÅI"
                );

                Destroy(projectile);
            }
        }
    }


    IEnumerator ShockwaveAttack()
    {
        if (isDead)
            yield break;

        Debug.Log("è’åÇîgÅFó\çêäJén");

        SetBossColor(warningColor);

        // ó\çêâ~Çï\é¶
        Vector3 center = transform.position;

        if (shockwaveLine != null)
        {
            shockwaveLine.transform.localPosition = Vector3.zero;
            shockwaveLine.startColor = shockwaveWarningColor;
            shockwaveLine.endColor = shockwaveWarningColor;
            shockwaveLine.enabled = true;

            DrawShockwaveCircle(shockwaveRadius);
        }

        // âÒîÇ∑ÇÈéûä‘
        yield return new WaitForSeconds(shockwaveWarningTime);

        if (isDead)
            yield break;

        Debug.Log("è’åÇîgÅFÉ_ÉÅÅ[ÉWîªíËäJén");

        if (player != null && player.currentHP > 0)
        {
            float distance = Vector2.Distance(
                center,
                player.transform.position
            );

            float hitRadius = shockwaveRadius + 1.5f;

            Debug.Log(
                $"è’åÇîgîªíËÅFãóó£={distance:F2}ÅAîÕàÕ={hitRadius:F2}"
            );

            if (distance <= hitRadius)
            {
                int damage = secondPhaseStarted
                    ? Mathf.RoundToInt(shockwaveDamage * 1.3f)
                    : shockwaveDamage;

                player.TakeDamage(damage);
                Debug.Log($"è’åÇîgñΩíÜÅI {damage} É_ÉÅÅ[ÉW");
            }
            else
            {
                Debug.Log("è’åÇîgâÒîÅI");
            }
        }

        // çUåÇå„ÇÃâ~å`ÉGÉtÉFÉNÉg
        SetBossColor(secondPhaseStarted
            ? phaseTwoColor
            : normalColor);

        float elapsed = 0f;

        while (elapsed < shockwaveVisualDuration && !isDead)
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

        if (shockwaveLine != null)
            shockwaveLine.enabled = false;
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
            // UpdateÇ≈ëÊ2å`ë‘Ç÷êÿÇËë÷Ç¶ÇÈ
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



