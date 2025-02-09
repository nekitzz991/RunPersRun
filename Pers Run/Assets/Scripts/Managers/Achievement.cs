using UnityEngine;
using UnityEngine.UI;
using System;

public enum AchievementType { Distance, Score, Items }

[Serializable]
public class Achievement
{
    public string id;
    public string name;
    public string description;
    public AchievementType type;  // Новое поле, указывающее тип достижения
    public float threshold;

    [HideInInspector]
    public float progress;
    public bool unlocked;

    public Image icon;
    public Sprite lockedSprite;
    public Sprite unlockedSprite;

    public void UpdateUI()
    {
        if (icon != null)
            icon.sprite = unlocked ? unlockedSprite : lockedSprite;
    }
}

