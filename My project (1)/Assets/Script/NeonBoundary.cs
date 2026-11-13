
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class NeonBoundary : MonoBehaviour
{
    [Header("移動範囲のサイズ")]
    public float width = 20f;
    public float height = 14f;

    [Header("枠の位置")]
    public Vector2 center = Vector2.zero;

    [Header("ネオン設定")]
    public Color neonColor = new Color(0.1f, 1f, 0.9f, 1f);
    public float lineWidth = 0.06f;
    public float glowWidth = 0.22f;
    public float pulseSpeed = 2f;

    private LineRenderer glowLine;
    private LineRenderer mainLine;
    private Material glowMaterial;
    private Material mainMaterial;


    public Vector2 MinBounds =>
        center - new Vector2(width / 2f, height / 2f);

    public Vector2 MaxBounds =>
        center + new Vector2(width / 2f, height / 2f);

    void Awake()
    {
        // 外側のぼんやりした光
        glowLine = GetComponent<LineRenderer>();
        SetupLine(glowLine, glowWidth);
        glowLine.startColor = new Color(
            neonColor.r, neonColor.g, neonColor.b, 0.2f);
        glowLine.endColor = glowLine.startColor;

        // 内側のはっきりした線
        GameObject inner = new GameObject("NeonBoundaryCore");
        inner.transform.SetParent(transform, false);

        mainLine = inner.AddComponent<LineRenderer>();
        SetupLine(mainLine, lineWidth);
        mainLine.startColor = neonColor;
        mainLine.endColor = neonColor;

        // 枠の頂点を設定
        UpdateCorners();
    }

    void SetupLine(LineRenderer line, float widthValue)
    {
        line.useWorldSpace = true;
        line.loop = true;
        line.positionCount = 4;
        line.startWidth = widthValue;
        line.endWidth = widthValue;
        line.numCapVertices = 4;
        line.numCornerVertices = 4;
        line.sortingOrder = 50;

        Shader shader = Shader.Find("Sprites/Default");

        if (shader != null)
        {
            Material material = new Material(shader);
            line.material = material;

            if (line == glowLine)
                glowMaterial = material;
            else
                mainMaterial = material;
        }
    }

    void UpdateCorners()
    {
        float halfW = width / 2f;
        float halfH = height / 2f;

        Vector3[] corners =
        {
            new Vector3(center.x - halfW, center.y - halfH, 0f),
            new Vector3(center.x - halfW, center.y + halfH, 0f),
            new Vector3(center.x + halfW, center.y + halfH, 0f),
            new Vector3(center.x + halfW, center.y - halfH, 0f)
        };

        glowLine.SetPositions(corners);
        mainLine.SetPositions(corners);
    }

    void Update()
    {
        // 光がゆっくり明滅する
        float pulse = 0.65f + Mathf.Sin(Time.time * pulseSpeed) * 0.2f;

        Color coreColor = neonColor;
        coreColor.a = pulse;

        Color glowColor = neonColor;
        glowColor.a = 0.12f + pulse * 0.12f;

        mainLine.startColor = coreColor;
        mainLine.endColor = coreColor;
        glowLine.startColor = glowColor;
        glowLine.endColor = glowColor;

        // Inspectorでサイズや位置を変更した場合にも更新
        UpdateCorners();
    }

    void OnDestroy()
    {
        if (glowMaterial != null)
            Destroy(glowMaterial);

        if (mainMaterial != null)
            Destroy(mainMaterial);
    }
}

