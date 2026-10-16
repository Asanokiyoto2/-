
using UnityEngine;

public class ExperienceOrb : MonoBehaviour
{
    public int value = 1;

    void OnTriggerEnter2D(Collider2D other)
    {
        LevelSystem levelSystem = other.GetComponent<LevelSystem>();

        if (levelSystem != null)
        {
            levelSystem.AddExperience(value);
            Destroy(gameObject);
        }
    }
}

