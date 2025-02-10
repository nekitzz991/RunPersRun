using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Achievements/Achievement Data", fileName = "NewAchievement")]
public class AchievementData : ScriptableObject
{
    [Header("Основные данные")]
    public string id;                
    public string achievementName;  
    public string description;      
    public float threshold;  // Порог выполнения (например, 500м или 10000м)

    [Header("Настройки отображения")]
    public Sprite lockedSprite;     
    public Sprite unlockedSprite;  

    [Header("Статус")]
    public bool isUnlocked = false; 

    public enum AchievementType
    {
        Distance,       // Дистанция (например, "пробежать 500м")
        Score,          // Очки (например, "набрать 1000 очков")
        ItemCollection  // Сбор предметов (кости, какашки)
    }

    [Header("Тип достижения")]
    public AchievementType achievementType;

    public enum ProgressType
    {
        SingleRun,     // Достижение за одну игру (например, пробежать 500 м в одном забеге)
        TotalProgress  // Достижение за весь прогресс (например, пробежать 10000 м за всё время)
    }

    [Header("Тип прогресса")]
    public ProgressType progressType;

    [Header("Скрытое достижение")]
    public bool isSecret; 

    [Header("События")]
    public UnityEvent onUnlocked;  

    public void CheckProgress(float value)
    {
        if (!isUnlocked && value >= threshold)
        {
            isUnlocked = true;
            onUnlocked?.Invoke();
            Debug.Log($"Достижение {achievementName} разблокировано!");
        }
    }
}
