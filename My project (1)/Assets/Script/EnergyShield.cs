
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LineRenderer))]
public class EnergyShield : MonoBehaviour
{
    [Header("シールド設定")]
    public float duration = 3f;
    public float cooldown = 8f;
    public float radius = 0.9f;

    [Header("見た目")]
    public Color shieldColor = new Color(0.1f, 0.9f, 1f, 1f);
    public float lineWidth = 0.08f;
    public float pulseSpeed = 5f;

    private LineRenderer glowLine;
    private LineRenderer coreLine;
    private Material glowMaterial;
    private Material coreMaterial;

    private float shieldEndTime;
    private float nextUseTime;
    private bool isActive;

    public bool IsActive => isActive;
    public float CooldownRemaining =>
        Mathf.Max(0f, nextUseTime - Time.time);

    void Awake()
    {
        // 外側の光
        glowLine = GetComponent<LineRenderer>();
        SetupLine(glowLine, lineWidth * 2.5f);
        glowLine.startColor = new Color(
            shieldColor.r, shieldColor.g, shieldColor.b, 0.25f);
        glowLine.endColor = glowLine.startColor;

        // 内側の明るい線
        GameObject core = new GameObject("ShieldCore");
        core.transform.SetParent(transform, false);

        coreLine = core.AddComponent<LineRenderer>();
        SetupLine(coreLine, lineWidth);
        coreLine.startColor = shieldColor;
        coreLine.endColor = shieldColor;

        DrawCircle();

        // 最初は非表示
        SetVisible(false);
    }

    void SetupLine(LineRenderer line, float widthValue)
    {
        line.useWorldSpace = false;
        line.loop = true;
        line.positionCount = 49;
        line.startWidth = widthValue;
        line.endWidth = widthValue;
        line.numCapVertices = 4;
        line.sortingOrder = 40;

        Shader shader = Shader.Find("Sprites/Default");

        if (shader != null)
        {
            Material material = new Material(shader);
            line.material = material;

            if (line == glowLine)
                glowMaterial = material;
            else
                coreMaterial = material;
        }
    }

    void DrawCircle()
    {
        for (int i = 0; i < coreLine.positionCount; i++)
        {
            float angle = i * Mathf.PI * 2f /
                          (coreLine.positionCount);

            Vector3 point = new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0f
            );

            glowLine.SetPosition(i, point);
            coreLine.SetPosition(i, point);
        }
    }

    void Update()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.IsGameOver)
            return;

        if (isActive && Time.time >= shieldEndTime)
        {
            isActive = false;
            SetVisible(false);
        }

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.qKey.wasPressedThisFrame &&
            !isActive &&
            Time.time >= nextUseTime)
        {
            ActivateShield();
        }

        if (isActive)
        {
            float pulse = 0.75f +
                Mathf.Sin(Time.time * pulseSpeed) * 0.25f;

            Color core = shieldColor;
            core.a = pulse;

            Color glow = shieldColor;
            glow.a = 0.15f + pulse * 0.15f;

            coreLine.startColor = core;
            coreLine.endColor = core;
            glowLine.startColor = glow;
            glowLine.endColor = glow;
        }
    }

    void ActivateShield()
    {
        isActive = true;
        shieldEndTime = Time.time + duration;
        nextUseTime = Time.time + duration + cooldown;

        SetVisible(true);

        Debug.Log("エネルギーシールド発動！");
    }

    void SetVisible(bool visible)
    {
        if (glowLine != null)
            glowLine.enabled = visible;

        if (coreLine != null)
            coreLine.enabled = visible;
    }

    void OnDestroy()
    {
        if (glowMaterial != null)
            Destroy(glowMaterial);

        if (coreMaterial != null)
            Destroy(coreMaterial);
    }
}

