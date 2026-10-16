
using UnityEngine;

public class LevelSystem : MonoBehaviour
{
    public int Level { get; private set; } = 1;
    public int Experience { get; private set; } = 0;
    public int ExperienceToNextLevel { get; private set; } = 5;

    private PlayerController player;

    void Awake()
    {
        player = GetComponent<PlayerController>();
    }

    public void AddExperience(int amount)
    {
        Experience += amount;

        while (Experience >= ExperienceToNextLevel)
        {
            Experience -= ExperienceToNextLevel;
            Level++;
            ExperienceToNextLevel += 3;

            if (player != null)
                player.Heal(20);

            Debug.Log("LEVEL UP! Level: " + Level);
        }
    }
}

