using UnityEngine;
using System.Collections.Generic;
using System;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    [Header("Список достижений")]
    public List<AchievementData> achievementDataList; // Заполняется через инспектор ассетами AchievementData

    public static event Action<AchievementData> OnAchievementUnlocked; // Глобальное событие

    private class AchievementState
    {
        public AchievementData data;
        public float progress;
        public bool unlocked;

        public AchievementState(AchievementData data)
        {
            this.data = data;
            progress = PlayerPrefs.GetFloat("AchievementProgress_" + data.id, 0); // Загружаем прогресс
            unlocked = PlayerPrefs.GetInt("Achievement_" + data.id, 0) == 1;
        }
    }

    private Dictionary<string, AchievementState> achievementStates;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Сохраняем AchievementManager между сценами

        achievementStates = new Dictionary<string, AchievementState>();
        foreach (AchievementData data in achievementDataList)
        {
            achievementStates[data.id] = new AchievementState(data);
        }
    }

    private void OnEnable()
    {
        OnAchievementUnlocked += HandleAchievementUnlocked;
    }

    private void OnDisable()
    {
        OnAchievementUnlocked -= HandleAchievementUnlocked;
    }

    /// <summary>
    /// Обработчик разблокированных достижений (можно подключить UI).
    /// </summary>
    private void HandleAchievementUnlocked(AchievementData data)
    {
        Debug.Log($"🏆 Обработано событие: {data.achievementName}");
    }

    /// <summary>
    /// Устанавливает конкретное значение прогресса достижения.
    /// </summary>
    public void UpdateAchievement(string id, float progress)
    {
        if (achievementStates.TryGetValue(id, out AchievementState state) && !state.unlocked)
        {
            state.progress = progress;
            PlayerPrefs.SetFloat("AchievementProgress_" + id, progress); // Сохраняем прогресс
            PlayerPrefs.Save();

            if (state.progress >= state.data.threshold)
            {
                UnlockAchievement(state);
            }
        }
    }

    /// <summary>
    /// Добавляет прогресс к существующему значению.
    /// </summary>
    public void AddProgress(string id, float amount)
    {
        if (achievementStates.TryGetValue(id, out AchievementState state) && !state.unlocked)
        {
            state.progress += amount;
            PlayerPrefs.SetFloat("AchievementProgress_" + id, state.progress); // Сохраняем прогресс
            PlayerPrefs.Save();

            if (state.progress >= state.data.threshold)
            {
                UnlockAchievement(state);
            }
        }
    }

    private void UnlockAchievement(AchievementState state)
    {
        state.unlocked = true;
        PlayerPrefs.SetInt("Achievement_" + state.data.id, 1);
        PlayerPrefs.Save();

        Debug.Log($"🎉 Достижение разблокировано: {state.data.achievementName}");

        // Вызов UnityEvent из ScriptableObject
        if (state.data.onUnlocked != null)
        {
            state.data.onUnlocked.Invoke();
            Debug.Log($"✅ Вызвано событие onUnlocked для: {state.data.achievementName}");
        }
        else
        {
            Debug.LogWarning($"⚠ Достижение {state.data.achievementName} не имеет событий в onUnlocked.");
        }

        // Вызов глобального события
        OnAchievementUnlocked?.Invoke(state.data);
    }

    /// <summary>
    /// Проверяет, разблокировано ли достижение.
    /// </summary>
    public bool IsAchievementUnlocked(string id)
    {
        return achievementStates.TryGetValue(id, out AchievementState state) && state.unlocked;
    }
}
