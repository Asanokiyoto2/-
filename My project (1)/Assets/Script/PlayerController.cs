
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Coroutine flashCoroutine;
    public float moveSpeed = 5f;
    public int maxHP = 100;
    public int currentHP = 100;

    [Header("移動範囲")]
    public NeonBoundary neonBoundary;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private EnergyShield energyShield;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        currentHP = maxHP;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (neonBoundary == null)
        {
            neonBoundary = FindFirstObjectByType<NeonBoundary>();
        }
        energyShield = GetComponent<EnergyShield>();
    }

    void Update()
    {
        moveInput = Vector2.zero;

        if (Keyboard.current == null) return;

        if (Keyboard.current.wKey.isPressed ||
            Keyboard.current.upArrowKey.isPressed)
            moveInput.y += 1f;

        if (Keyboard.current.sKey.isPressed ||
            Keyboard.current.downArrowKey.isPressed)
            moveInput.y -= 1f;

        if (Keyboard.current.aKey.isPressed ||
            Keyboard.current.leftArrowKey.isPressed)
            moveInput.x -= 1f;

        if (Keyboard.current.dKey.isPressed ||
            Keyboard.current.rightArrowKey.isPressed)
            moveInput.x += 1f;

        moveInput = moveInput.normalized;
    }


    void FixedUpdate()
    {
        if (currentHP <= 0) return;

        Vector2 nextPosition =
            rb.position + moveInput * moveSpeed * Time.fixedDeltaTime;

        if (neonBoundary != null)
        {
            Vector2 min = neonBoundary.MinBounds;
            Vector2 max = neonBoundary.MaxBounds;

            // プレイヤーの中心が枠の内側に収まるようにする
            Vector2 halfSize = Vector2.zero;

            if (spriteRenderer != null)
            {
                halfSize = spriteRenderer.bounds.extents;
            }

            nextPosition.x = Mathf.Clamp(
                nextPosition.x,
                min.x + halfSize.x,
                max.x - halfSize.x
            );

            nextPosition.y = Mathf.Clamp(
                nextPosition.y,
                min.y + halfSize.y,
                max.y - halfSize.y
            );
        }

        rb.MovePosition(nextPosition);
    }


    public void TakeDamage(int damage)
    {
        if (currentHP <= 0) return;

        // シールド発動中はダメージを受けない
        if (energyShield != null && energyShield.IsActive)
            return;

        currentHP -= damage;
        currentHP = Mathf.Max(0, currentHP);

        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(DamageFlash());

        if (currentHP <= 0)
        {
            GameManager.Instance?.GameOver();
        }
    }

    public void Heal(int amount)
    {
        currentHP = Mathf.Min(maxHP, currentHP + amount);
    }

    IEnumerator DamageFlash()
    {
        if (spriteRenderer == null) yield break;

        for (int i = 0; i < 3; i++)
        {
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.06f);

            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.06f);
        }

        spriteRenderer.color = Color.blue;
        flashCoroutine = null;
    }
}

