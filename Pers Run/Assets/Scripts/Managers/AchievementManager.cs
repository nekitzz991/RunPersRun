using UnityEngine;
using System.Collections.Generic;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    [Header("Список достижений")]
    public List<Achievement> achievements; // Заполните список в инспекторе

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        LoadAchievements();
    }

    /// <summary>
    /// Обновляет прогресс достижения по его id. Если достигнут порог, открывает достижение.
    /// </summary>
    /// <param name="id">Идентификатор достижения</param>
    /// <param name="progress">Новое значение прогресса</param>
    public void UpdateAchievement(string id, float progress)
    {
        Achievement ach = achievements.Find(a => a.id == id);
        if (ach != null && !ach.unlocked)
        {
            ach.progress = progress;
            if (ach.progress >= ach.threshold)
            {
                UnlockAchievement(ach);
            }
        }
    }

    private void UnlockAchievement(Achievement achievement)
    {
        achievement.unlocked = true;
        achievement.UpdateUI();
        // Сохраняем состояние достижения
        PlayerPrefs.SetInt("Achievement_" + achievement.id, 1);
        PlayerPrefs.Save();
        Debug.Log("Достижение открыто: " + achievement.name);
    }

    /// <summary>
    /// Загружает состояние достижений из PlayerPrefs
    /// </summary>
    public void LoadAchievements()
    {
        foreach (Achievement ach in achievements)
        {
            ach.unlocked = PlayerPrefs.GetInt("Achievement_" + ach.id, 0) == 1;
            ach.UpdateUI();
        }
    }
}
