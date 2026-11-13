
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BulletController : MonoBehaviour
{
    public float speed = 8f;
    public float lifeTime = 3f;

    private Rigidbody2D rb;
    private Vector2 direction;
    private int damage = 10;
    private bool launched;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    public void Launch(Vector2 shotDirection, int shotDamage)
    {
        direction = shotDirection.normalized;
        damage = shotDamage;
        launched = true;

        Destroy(gameObject, lifeTime);
    }

    void FixedUpdate()
    {
        if (!launched)
            return;

        rb.MovePosition(
            rb.position + direction * speed * Time.fixedDeltaTime
        );
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        EnemyController enemy =
            other.GetComponentInParent<EnemyController>();

        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        BossController boss =
            other.GetComponentInParent<BossController>();

        if (boss != null)
        {
            boss.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}


