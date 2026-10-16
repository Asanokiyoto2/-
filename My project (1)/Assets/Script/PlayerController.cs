
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public int maxHP = 100;
    public int currentHP = 100;

    public Vector2 arenaMin = new Vector2(-8f, -4.3f);
    public Vector2 arenaMax = new Vector2(8f, 4.3f);

    private Rigidbody2D rb;
    private Vector2 moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        currentHP = maxHP;
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

        nextPosition.x = Mathf.Clamp(
            nextPosition.x, arenaMin.x, arenaMax.x);

        nextPosition.y = Mathf.Clamp(
            nextPosition.y, arenaMin.y, arenaMax.y);

        rb.MovePosition(nextPosition);
    }

    public void TakeDamage(int damage)
    {
        if (currentHP <= 0) return;

        currentHP -= damage;
        currentHP = Mathf.Max(0, currentHP);

        if (currentHP <= 0)
        {
            GameManager.Instance?.GameOver();
        }
    }

    public void Heal(int amount)
    {
        currentHP = Mathf.Min(maxHP, currentHP + amount);
    }
}

