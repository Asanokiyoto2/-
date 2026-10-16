
using UnityEngine;

public class LevelUpUI : MonoBehaviour
{
    public static LevelUpUI Instance { get; private set; }

    private LevelSystem currentLevelSystem;
    private bool isShowing;

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
        isShowing = true;
        Time.timeScale = 0f;
    }

    void OnGUI()
    {
        if (!isShowing) return;

        float panelWidth = 420f;
        float panelHeight = 250f;

        float x = (Screen.width - panelWidth) / 2f;
        float y = (Screen.height - panelHeight) / 2f;

        GUI.Box(
            new Rect(x, y, panelWidth, panelHeight),
            "LEVEL UP!"
        );

        GUI.Label(
            new Rect(x + 20, y + 35, panelWidth - 40, 30),
            "Choose one upgrade"
        );

        if (GUI.Button(
            new Rect(x + 20, y + 80, panelWidth - 40, 40),
            "1. Attack Damage +10"))
        {
            SelectUpgrade(0);
        }

        if (GUI.Button(
            new Rect(x + 20, y + 130, panelWidth - 40, 40),
            "2. Attack Speed Up"))
        {
            SelectUpgrade(1);
        }

        if (GUI.Button(
            new Rect(x + 20, y + 180, panelWidth - 40, 40),
            "3. Heal HP +30"))
        {
            SelectUpgrade(2);
        }
    }

    void SelectUpgrade(int choice)
    {
        if (currentLevelSystem != null)
            currentLevelSystem.ApplyUpgrade(choice);

        isShowing = false;
        currentLevelSystem = null;
        Time.timeScale = 1f;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        Time.timeScale = 1f;
    }
}

