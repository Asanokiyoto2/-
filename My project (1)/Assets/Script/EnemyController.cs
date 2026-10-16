
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    public float moveSpeed = 2f;
    public int maxHP = 30;
    public int contactDamage = 10;
    public int scoreValue = 10;
    public int experienceValue = 1;
    public float damageInterval = 1f;
    public GameObject experiencePrefab;

    private int currentHP;
    private Rigidbody2D rb;
    private PlayerController player;
    private float nextDamageTime;

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
    }

    void FixedUpdate()
    {
        if (player == null || player.currentHP <= 0) return;

        Vector2 direction =
            ((Vector2)player.transform.position - rb.position).normalized;

        rb.MovePosition(
            rb.position + direction * moveSpeed * Time.fixedDeltaTime);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        PlayerController target = other.GetComponent<PlayerController>();

        if (target != null && Time.time >= nextDamageTime)
        {
            target.TakeDamage(contactDamage);
            nextDamageTime = Time.time + damageInterval;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;

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
                experiencePrefab, transform.position, Quaternion.identity);

            ExperienceOrb xp = orb.GetComponent<ExperienceOrb>();

            if (xp != null)
                xp.value = experienceValue;
        }

        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(scoreValue);

        Destroy(gameObject);
    }
}

