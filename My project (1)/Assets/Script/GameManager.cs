
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int Score { get; private set; }
    public bool IsGameOver { get; private set; }

    private float startTime;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Time.timeScale = 1f;
    }

    void Start()
    {
        startTime = Time.time;
        Score = 0;
        IsGameOver = false;
    }

    public void AddScore(int amount)
    {
        if (!IsGameOver)
            Score += amount;
    }

    public void GameOver()
    {
        if (IsGameOver) return;

        IsGameOver = true;
        Debug.Log("GAME OVER");
    }

    void Update()
    {
        if (IsGameOver && KeyboardRestartPressed())
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(
                SceneManager.GetActiveScene().buildIndex);
        }
    }

    bool KeyboardRestartPressed()
    {
        return UnityEngine.InputSystem.Keyboard.current != null &&
               UnityEngine.InputSystem.Keyboard.current.rKey.wasPressedThisFrame;
    }

    void OnGUI()
    {
        PlayerController player =
            FindFirstObjectByType<PlayerController>();

        LevelSystem level =
            player != null ? player.GetComponent<LevelSystem>() : null;

        float elapsed = Time.time - startTime;

        GUI.Label(new Rect(15, 15, 250, 25),
            "SCORE : " + Score);

        GUI.Label(new Rect(15, 40, 250, 25),
            "TIME : " + Mathf.FloorToInt(elapsed) + " sec");

        if (player != null)
        {
            GUI.Label(new Rect(15, 65, 300, 25),
                "HP : " + player.currentHP + " / " + player.maxHP);
        }

        if (level != null)
        {
            GUI.Label(new Rect(15, 90, 350, 25),
                "LEVEL : " + level.Level +
                "   EXP : " + level.Experience +
                " / " + level.ExperienceToNextLevel);
        }

        if (IsGameOver)
        {
            GUI.Box(new Rect(Screen.width / 2 - 150,
                Screen.height / 2 - 60, 300, 120), "GAME OVER");

            GUI.Label(new Rect(Screen.width / 2 - 100,
                Screen.height / 2 - 20, 250, 25),
                "FINAL SCORE : " + Score);

            GUI.Label(new Rect(Screen.width / 2 - 100,
                Screen.height / 2 + 10, 250, 25),
                "Press R to Restart");
        }
    }
}

