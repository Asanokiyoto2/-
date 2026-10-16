
using UnityEngine;

public class GameHUD : MonoBehaviour
{
    public PlayerController player;
    public LevelSystem levelSystem;

    public float barWidth = 240f;
    public float barHeight = 18f;

    private GUIStyle labelStyle;
    private GUIStyle titleStyle;

    void Start()
    {
        FindPlayer();
    }

    void FindPlayer()
    {
        if (player == null)
            player = FindFirstObjectByType<PlayerController>();

        if (player != null && levelSystem == null)
            levelSystem = player.GetComponent<LevelSystem>();
    }

    void OnGUI()
    {
        if (player == null)
            FindPlayer();

        if (player == null) return;

        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 14;
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.normal.textColor = Color.white;

            titleStyle = new GUIStyle(labelStyle);
            titleStyle.fontSize = 18;
        }

        float x = Screen.width - barWidth - 20f;
        float y = 20f;

        // HPバー
        DrawBar(
            x, y,
            barWidth, barHeight,
            (float)player.currentHP / Mathf.Max(1, player.maxHP),
            new Color(0.9f, 0.15f, 0.2f),
            "HP  " + player.currentHP + " / " + player.maxHP
        );

        y += 42f;

        // 経験値バー
        if (levelSystem != null)
        {
            float expPercent =
                (float)levelSystem.Experience /
                Mathf.Max(1, levelSystem.ExperienceToNextLevel);

            DrawBar(
                x, y,
                barWidth, barHeight,
                expPercent,
                new Color(1f, 0.75f, 0.1f),
                "LEVEL " + levelSystem.Level +
                "   EXP " + levelSystem.Experience +
                " / " + levelSystem.ExperienceToNextLevel
            );
        }

        // ボスHPバー
        BossController boss =
            FindFirstObjectByType<BossController>();

        if (boss != null)
        {
            float bossWidth = Mathf.Min(360f, Screen.width - 40f);
            float bossX = (Screen.width - bossWidth) / 2f;
            float bossY = 65f;

            DrawBar(
                bossX, bossY,
                bossWidth, barHeight + 4f,
                boss.HPPercent,
                new Color(0.85f, 0.05f, 0.1f),
                "BOSS  " + boss.currentHP + " / " + boss.maxHP
            );
        }
    }

    void DrawBar(
        float x,
        float y,
        float width,
        float height,
        float percent,
        Color fillColor,
        string label)
    {
        percent = Mathf.Clamp01(percent);

        // 背景
        GUI.color = new Color(0.12f, 0.12f, 0.12f, 0.95f);
        GUI.DrawTexture(
            new Rect(x, y, width, height),
            Texture2D.whiteTexture
        );

        // 枠の内側にバーを描く
        float padding = 2f;
        float innerWidth = (width - padding * 2f) * percent;

        GUI.color = fillColor;
        GUI.DrawTexture(
            new Rect(
                x + padding,
                y + padding,
                innerWidth,
                height - padding * 2f
            ),
            Texture2D.whiteTexture
        );

        // 枠線
        GUI.color = Color.white;
        GUI.DrawTexture(
            new Rect(x, y, width, 1f),
            Texture2D.whiteTexture
        );
        GUI.DrawTexture(
            new Rect(x, y + height - 1f, width, 1f),
            Texture2D.whiteTexture
        );
        GUI.DrawTexture(
            new Rect(x, y, 1f, height),
            Texture2D.whiteTexture
        );
        GUI.DrawTexture(
            new Rect(x + width - 1f, y, 1f, height),
            Texture2D.whiteTexture
        );

        // ラベル
        GUI.Label(
            new Rect(x, y - 19f, width + 100f, 20f),
            label,
            labelStyle
        );

        GUI.color = Color.white;
    }
}

