
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyProjectile : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 direction;
    private float speed = 5f;
    private int damage = 10;
    private bool launched;

    public float lifeTime = 5f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.freezeRotation = true;
    }

    public void Launch(Vector2 shotDirection, float shotSpeed, int shotDamage)
    {
        direction = shotDirection.normalized;
        speed = shotSpeed;
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
        PlayerController target =
            other.GetComponentInParent<PlayerController>();

        if (target != null)
        {
            target.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}

