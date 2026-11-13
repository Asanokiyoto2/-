
using UnityEngine;

public class ShockwaveUI : MonoBehaviour
{
    public ShockwaveSkill shockwaveSkill;

    [Header("表示位置")]
    public Vector2 position = new Vector2(20f, 100f);
    public float boxSize = 150f;

    private GUIStyle titleStyle;
    private GUIStyle statusStyle;
    private GUIStyle keyStyle;
    private GUIStyle timeStyle;

    void Start()
    {
        if (shockwaveSkill == null)
        {
            shockwaveSkill =
                FindFirstObjectByType<ShockwaveSkill>();
        }
    }

    void OnGUI()
    {
        if (shockwaveSkill == null) return;

        SetupStyles();

        float x = position.x;
        float y = Screen.height - position.y - 82f;

        // パネル背景
        GUI.Box(
            new Rect(x, y, boxSize, 82f),
            GUIContent.none
        );

        bool ready = shockwaveSkill.IsReady;
        float progress = shockwaveSkill.CooldownProgress;

        // Eキー表示
        GUI.color = ready
            ? new Color(0.2f, 0.85f, 1f)
            : new Color(0.45f, 0.45f, 0.5f);

        GUI.Box(
            new Rect(x + 8f, y + 14f, 42f, 42f),
            "E",
            keyStyle
        );

        GUI.color = Color.white;

        GUI.Label(
            new Rect(x + 58f, y + 8f, boxSize - 65f, 24f),
            "衝撃波",
            titleStyle
        );

        string status = ready
            ? "READY"
            : "COOLDOWN";

        GUI.Label(
            new Rect(x + 58f, y + 30f, boxSize - 65f, 20f),
            status,
            statusStyle
        );

        if (!ready)
        {
            GUI.Label(
                new Rect(x + 58f, y + 48f, boxSize - 65f, 18f),
                shockwaveSkill.CooldownRemaining.ToString("F1")
                    + " 秒",
                timeStyle
            );
        }

        // クールダウンゲージ
        Rect bar = new Rect(x + 8f, y + 62f, boxSize - 16f, 10f);

        GUI.color = new Color(0.15f, 0.18f, 0.23f);
        GUI.DrawTexture(bar, Texture2D.whiteTexture);

        GUI.color = ready
            ? new Color(0.2f, 0.85f, 1f)
            : new Color(0.55f, 0.55f, 0.65f);

        GUI.DrawTexture(
            new Rect(bar.x, bar.y, bar.width * progress, bar.height),
            Texture2D.whiteTexture
        );

        GUI.color = Color.white;
    }

    void SetupStyles()
    {
        if (titleStyle != null) return;

        titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 15;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = Color.white;

        statusStyle = new GUIStyle(GUI.skin.label);
        statusStyle.fontSize = 12;
        statusStyle.fontStyle = FontStyle.Bold;

        keyStyle = new GUIStyle(GUI.skin.box);
        keyStyle.fontSize = 22;
        keyStyle.fontStyle = FontStyle.Bold;
        keyStyle.alignment = TextAnchor.MiddleCenter;
        keyStyle.normal.textColor = Color.white;

        timeStyle = new GUIStyle(GUI.skin.label);
        timeStyle.fontSize = 11;
        timeStyle.normal.textColor = Color.white;
    }
}

