
using UnityEngine;

public class LevelSystem : MonoBehaviour
{
    public int Level { get; private set; } = 1;
    public int Experience { get; private set; } = 0;
    public int ExperienceToNextLevel { get; private set; } = 5;

    private PlayerController player;
    private AutoShooter autoShooter;

    private bool isLevelingUp;
    private int pendingLevelUps;

    public bool HasPendingLevelUps => pendingLevelUps > 0;

    void Awake()
    {
        player = GetComponent<PlayerController>();
        autoShooter = GetComponent<AutoShooter>();
    }

    public void AddExperience(int amount)
    {
        if (amount <= 0) return;

        Experience += amount;

        // 一度に大量の経験値を獲得しても対応
        while (Experience >= ExperienceToNextLevel)
        {
            Experience -= ExperienceToNextLevel;

            Level++;
            pendingLevelUps++;

            ExperienceToNextLevel += 3;
        }

        if (pendingLevelUps > 0 && !isLevelingUp)
        {
            isLevelingUp = true;

            if (LevelUpUI.Instance != null)
            {
                LevelUpUI.Instance.ShowChoices(this);
            }
            else
            {
                // UIがなければランダムな強化を適用
                while (pendingLevelUps > 0)
                {
                    ApplyUpgrade(Random.Range(0, 5));
                }
            }
        }
    }

    public void ApplyUpgrade(int choice)
    {
        switch (choice)
        {
            case 0:
                // 攻撃力アップ
                if (autoShooter != null)
                    autoShooter.UpgradeDamage();
                break;

            case 1:
                // 連射速度アップ
                if (autoShooter != null)
                    autoShooter.UpgradeAttackSpeed();
                break;

            case 2:
                // HP回復
                if (player != null)
                    player.Heal(30);
                break;

            case 3:
                // 移動速度アップ
                if (player != null)
                {
                    player.moveSpeed += 0.5f;
                    Debug.Log(
                        "移動速度アップ！ 現在：" + player.moveSpeed
                    );
                }
                break;

            case 4:
                // 最大HPアップ＋HPを20回復
                if (player != null)
                {
                    player.maxHP += 20;
                    player.currentHP = Mathf.Min(
                        player.currentHP + 20,
                        player.maxHP
                    );

                    //Debug.Log("最大HPアップ！ 最大HP：" + player.maxHP);
                }
                break;
        }

        pendingLevelUps = Mathf.Max(0, pendingLevelUps - 1);

        if (pendingLevelUps <= 0)
            isLevelingUp = false;
    }
}



