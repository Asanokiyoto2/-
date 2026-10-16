
using UnityEngine;

public class LevelSystem : MonoBehaviour
{
    public int Level { get; private set; } = 1;
    public int Experience { get; private set; } = 0;
    public int ExperienceToNextLevel { get; private set; } = 5;

    private PlayerController player;
    private AutoShooter autoShooter;
    private bool isLevelingUp;

    void Awake()
    {
        player = GetComponent<PlayerController>();
        autoShooter = GetComponent<AutoShooter>();
    }

    public void AddExperience(int amount)
    {
        if (amount <= 0) return;

        Experience += amount;

        if (Experience >= ExperienceToNextLevel)
        {
            Experience -= ExperienceToNextLevel;
            Level++;
            ExperienceToNextLevel += 3;

            if (!isLevelingUp)
            {
                isLevelingUp = true;

                if (LevelUpUI.Instance != null)
                {
                    LevelUpUI.Instance.ShowChoices(this);
                }
                else
                {
                    // ëIëâÊñ Ç™ñ¢ê›íËÇ≈Ç‡ÉQÅ[ÉÄÇ™é~Ç‹ÇÁÇ»Ç¢ÇÊÇ§Ç…Ç∑ÇÈ
                    ApplyUpgrade(Random.Range(0, 3));
                }
            }
        }
    }

    public void ApplyUpgrade(int choice)
    {
        switch (choice)
        {
            case 0:
                if (autoShooter != null)
                    autoShooter.UpgradeDamage();
                break;

            case 1:
                if (autoShooter != null)
                    autoShooter.UpgradeAttackSpeed();
                break;

            case 2:
                if (player != null)
                    player.Heal(30);
                break;
        }

        isLevelingUp = false;
    }
}


