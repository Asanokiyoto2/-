

using UnityEngine;

public class LevelUpUI : MonoBehaviour
{
    public static LevelUpUI Instance { get; private set; }

    private LevelSystem currentLevelSystem;
    private bool isShowing;

    private int[] choices = new int[3];

    private readonly string[] upgradeNames =
    {
        "攻撃力アップ",
        "連射速度アップ",
        "HP回復 +30",
        "移動速度アップ",
        "最大HP +20"
    };

    private readonly string[] upgradeDescriptions =
    {
        "弾のダメージ +10",
        "発射間隔を短縮",
        "HPを30回復する",
        "移動速度 +0.5",
        "最大HPと現在HPを増加"
    };

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void ShowChoices(LevelSystem levelSystem)
    {
        currentLevelSystem = levelSystem;

        GenerateChoices();

        isShowing = true;
        Time.timeScale = 0f;
    }

    void GenerateChoices()
    {
        // 5種類の候補を用意
        int[] pool = { 0, 1, 2, 3, 4 };

        // Fisher-Yatesシャッフル
        for (int i = pool.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);

            int temp = pool[i];
            pool[i] = pool[j];
            pool[j] = temp;
        }

        // シャッフル後の先頭3つを採用
        for (int i = 0; i < choices.Length; i++)
        {
            choices[i] = pool[i];
        }
    }

    void OnGUI()
    {
        if (!isShowing) return;

        float panelWidth = 460f;
        float panelHeight = 330f;

        float x = (Screen.width - panelWidth) / 2f;
        float y = (Screen.height - panelHeight) / 2f;

        GUI.Box(
            new Rect(x, y, panelWidth, panelHeight),
            "LEVEL UP!"
        );

        GUI.Label(
            new Rect(x + 20, y + 35, panelWidth - 40, 30),
            "強化を1つ選択してください"
        );

        for (int i = 0; i < choices.Length; i++)
        {
            int choice = choices[i];

            float buttonY = y + 80 + i * 75;

            GUI.Box(
                new Rect(x + 20, buttonY, panelWidth - 40, 65),
                ""
            );

            GUI.Label(
                new Rect(x + 32, buttonY + 7, panelWidth - 64, 22),
                (i + 1) + ". " + upgradeNames[choice]
            );

            GUI.Label(
                new Rect(x + 32, buttonY + 32, panelWidth - 64, 22),
                upgradeDescriptions[choice]
            );

            if (GUI.Button(
                new Rect(x + 20, buttonY, panelWidth - 40, 65),
                ""
            ))
            {
                SelectUpgrade(choice);
            }
        }
    }

    void SelectUpgrade(int choice)
    {
        if (currentLevelSystem == null) return;

        currentLevelSystem.ApplyUpgrade(choice);

        // 連続レベルアップが残っていれば次の選択画面へ
        if (currentLevelSystem.HasPendingLevelUps)
        {
            ShowChoices(currentLevelSystem);
        }
        else
        {
            isShowing = false;
            currentLevelSystem = null;
            Time.timeScale = 1f;
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        Time.timeScale = 1f;
    }
}


