using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AchievementsManager : MonoBehaviour
{
    // ========= Параметры достижений за ТЕКУЩУЮ ИГРУ =========
    [Header("Статистика текущей игры")]
    [Tooltip("Пройденное расстояние (в метрах) за игру")]
    public int currentDistance;
    [Tooltip("Набранные очки за игру")]
    public int currentScore;
    [Tooltip("Собрано костей за игру")]
    public int currentBones;
    [Tooltip("Собрано золотых костей за игру")]
    public int currentGoldenBones;
    [Tooltip("Количество 'какашек', на которые наступил игрок за игру")]
    public int currentPoops;

    // ========= Параметры достижений за ОБЩИЙ ПРОГРЕСС =========
    [Header("Общая статистика (за всё время)")]
    [Tooltip("Общее пройденное расстояние (в метрах)")]
    public int totalDistance;
    [Tooltip("Общее количество очков")]
    public int totalScore;
    [Tooltip("Общее количество собранных костей")]
    public int totalBones;
    [Tooltip("Общее количество собранных золотых костей")]
    public int totalGoldenBones;
    [Tooltip("Общее количество 'какашек', на которые наступил игрок")]
    public int totalPoops;

    // ========= Настройка UI для достижений =========
    [Header("UI для достижений")]
    [Tooltip("Текст-счётчик достижений (например, ( 3 / 22 ))")]
    [SerializeField] private Text counterText;
    [Tooltip("Текст процентного прогресса (например, 45.5% COMPLETE)")]
    [SerializeField] private Text percentualProgressText;
    [Tooltip("Список маркеров (иконок) для каждого достижения")]
    [SerializeField] private List<GameObject> markers = new List<GameObject>();

    // ========= Данные достижений =========
    [Header("Достижения")]
    [Tooltip("Количество полученных достижений")]
    public int achievCount;
    [Tooltip("Процент выполнения достижений")]
    public float percentualProgress;

    // Вспомогательные списки для сохранения состояния достижений (длина должна быть равна количеству достижений)
    public List<bool> isAchieved = new List<bool>();
    public List<int> isAchievedValue = new List<int>();

    // ========= Аудио =========
    [Header("Аудио для достижений")]
    [Tooltip("Аудиоклип, проигрываемый при получении достижения")]
    [SerializeField] private AudioClip achievementSound;
    [Tooltip("Громкость звука достижения")]
    [SerializeField] private float achievementSoundVolume = 1f;

    // Общее количество достижений (из условия – 22)
    private int totalAchievements = 22;
    // Прибавка к проценту за каждое достижение (100 / 22)
    private float progressIncrement;

    private void Start()
    {
        progressIncrement = 100f / totalAchievements;

        // Если списки не инициализированы нужным количеством элементов – заполняем их начальными значениями
        for (int i = isAchieved.Count; i < totalAchievements; i++)
        {
            isAchieved.Add(false);
            isAchievedValue.Add(0);
        }

        LoadPlayerPrefs();
    }

    private void Update()
    {
        CheckCurrentGameAchievements();
        CheckOverallAchievements();
        UpdateMarkers();
        UpdateProgressUI();
        SavePlayerPrefs();
    }

    #region Проверка достижений

    /// <summary>
    /// Проверка достижений, основанных на результатах текущей игры (индексы 0–10).
    /// </summary>
    private void CheckCurrentGameAchievements()
    {
        // --- Дистанция ---
        // 0. 🏃‍♂️ "Первый рывок" – пробежать 100 м.
        if (currentDistance >= 100 && !isAchieved[0])
            UnlockAchievement(0);
        // 1. 🔥 "Спринтер" – пробежать 1000 м.
        if (currentDistance >= 1000 && !isAchieved[1])
            UnlockAchievement(1);
        // 2. 🌍 "Марафонец" – пробежать 5000 м.
        if (currentDistance >= 5000 && !isAchieved[2])
            UnlockAchievement(2);

        // --- Счёт ---
        // 3. 🎯 "Начинающий" – набрать 100 очков.
        if (currentScore >= 100 && !isAchieved[3])
            UnlockAchievement(3);
        // 4. 💯 "Опытный" – набрать 500 очков.
        if (currentScore >= 500 && !isAchieved[4])
            UnlockAchievement(4);
        // 5. 🥇 "Мастер" – набрать 1000 очков.
        if (currentScore >= 1000 && !isAchieved[5])
            UnlockAchievement(5);
        // 6. 👑 "Легенда" – набрать 5000 очков.
        if (currentScore >= 5000 && !isAchieved[6])
            UnlockAchievement(6);

        // --- Сбор предметов ---
        // 7. 🍖 "Косточка для Перса" – собрать 10 костей.
        if (currentBones >= 10 && !isAchieved[7])
            UnlockAchievement(7);
        // 8. ✨ "Золотая удача" – собрать 5 золотых костей.
        if (currentGoldenBones >= 5 && !isAchieved[8])
            UnlockAchievement(8);
        // 9. 💩 "Осторожно!" – наступить на 5 какашек.
        if (currentPoops >= 5 && !isAchieved[9])
            UnlockAchievement(9);
        // 10. 🚽 "Невезение" – наступить на 10 какашек.
        if (currentPoops >= 10 && !isAchieved[10])
            UnlockAchievement(10);
    }

    /// <summary>
    /// Проверка достижений, основанных на общем прогрессе (индексы 11–21).
    /// </summary>
    private void CheckOverallAchievements()
    {
        // --- Дистанция ---
        // 11. 🌎 "Исследователь" – пробежать 10 000 м.
        if (totalDistance >= 10000 && !isAchieved[11])
            UnlockAchievement(11);
        // 12. 🚴 "Путешественник" – пробежать 50 000 м.
        if (totalDistance >= 50000 && !isAchieved[12])
            UnlockAchievement(12);
        // 13. 🚄 "Скоростной гонщик" – пробежать 100 000 м.
        if (totalDistance >= 100000 && !isAchieved[13])
            UnlockAchievement(13);

        // --- Счёт ---
        // 14. 📊 "Стабильный игрок" – заработать 10 000 очков суммарно.
        if (totalScore >= 10000 && !isAchieved[14])
            UnlockAchievement(14);
        // 15. 🏆 "Фанат рекордов" – заработать 50 000 очков суммарно.
        if (totalScore >= 50000 && !isAchieved[15])
            UnlockAchievement(15);
        // 16. 🏅 "Грандмастер" – заработать 100 000 очков суммарно.
        if (totalScore >= 100000 && !isAchieved[16])
            UnlockAchievement(16);

        // --- Сбор предметов ---
        // 17. 🦴 "Любитель костей" – собрать 100 костей.
        if (totalBones >= 100 && !isAchieved[17])
            UnlockAchievement(17);
        // 18. 🏆 "Собиратель костей" – собрать 1000 костей.
        if (totalBones >= 1000 && !isAchieved[18])
            UnlockAchievement(18);
        // 19. 💎 "Золотая лапка" – собрать 50 золотых костей.
        if (totalGoldenBones >= 50 && !isAchieved[19])
            UnlockAchievement(19);
        // 20. 💩 "Неуклюжий" – наступить на 50 какашек.
        if (totalPoops >= 50 && !isAchieved[20])
            UnlockAchievement(20);
        // 21. 🤢 "Грязнуля" – наступить на 100 какашек.
        if (totalPoops >= 100 && !isAchieved[21])
            UnlockAchievement(21);
    }

    /// <summary>
    /// Универсальный метод разблокировки достижения по его индексу.
    /// Помимо установки флага достижения, он обновляет счётчик, процентный прогресс
    /// и проигрывает звуковой эффект через AudioManager.
    /// </summary>
    /// <param name="index">Индекс достижения (от 0 до 21)</param>
    private void UnlockAchievement(int index)
    {
        isAchieved[index] = true;
        isAchievedValue[index] = 1;
        achievCount++;
        percentualProgress += progressIncrement;

        // Воспроизведение звука достижения через AudioManager
        if (AudioManager.Instance != null && achievementSound != null)
        {
            AudioManager.Instance.PlaySFXSound(achievementSound, achievementSoundVolume);
        }
    }

    #endregion

    #region Обновление UI

    /// <summary>
    /// Активирует UI‑маркер для каждого полученного достижения и обновляет текст счётчика.
    /// </summary>
    private void UpdateMarkers()
    {
        for (int i = 0; i < markers.Count && i < isAchieved.Count; i++)
        {
            if (isAchieved[i])
            {
                markers[i].SetActive(true);
            }
        }
        if (achievCount > totalAchievements)
            achievCount = totalAchievements;
        if (counterText != null)
            counterText.text = "( " + achievCount + " / " + totalAchievements + " )";
    }

    /// <summary>
    /// Обновляет текст процентного прогресса.
    /// </summary>
    private void UpdateProgressUI()
    {
        if (percentualProgress > 100f)
            percentualProgress = 100f;
        if (percentualProgressText != null)
            percentualProgressText.text = percentualProgress.ToString("F1") + "% COMPLETE";
    }

    #endregion

    #region Сохранение и загрузка данных через PlayerPrefs

    public bool IntToBool(int a)
    {
        return a != 0;
    }

    public int BoolToInt(bool b)
    {
        return b ? 1 : 0;
    }

    private void LoadPlayerPrefs()
    {
        achievCount = PlayerPrefs.GetInt("achievCount", 0);
        percentualProgress = PlayerPrefs.GetFloat("PercentualProgress", 0f);

        for (int i = 0; i < totalAchievements; i++)
        {
            int value = PlayerPrefs.GetInt("IsAchieved" + i, 0);
            if (i < isAchieved.Count)
                isAchieved[i] = IntToBool(value);
            else
                isAchieved.Add(IntToBool(value));

            if (i < isAchievedValue.Count)
                isAchievedValue[i] = value;
            else
                isAchievedValue.Add(value);
        }

        // Загрузка статистики (при необходимости статистику можно обновлять из GameManager)
        currentDistance = PlayerPrefs.GetInt("CurrentDistance", 0);
        currentScore = PlayerPrefs.GetInt("CurrentScore", 0);
        currentBones = PlayerPrefs.GetInt("CurrentBones", 0);
        currentGoldenBones = PlayerPrefs.GetInt("CurrentGoldenBones", 0);
        currentPoops = PlayerPrefs.GetInt("CurrentPoops", 0);

        totalDistance = PlayerPrefs.GetInt("TotalDistance", 0);
        totalScore = PlayerPrefs.GetInt("TotalScore", 0);
        totalBones = PlayerPrefs.GetInt("TotalBones", 0);
        totalGoldenBones = PlayerPrefs.GetInt("TotalGoldenBones", 0);
        totalPoops = PlayerPrefs.GetInt("TotalPoops", 0);
    }

    private void SavePlayerPrefs()
    {
        PlayerPrefs.SetInt("achievCount", achievCount);
        PlayerPrefs.SetFloat("PercentualProgress", percentualProgress);

        for (int i = 0; i < totalAchievements; i++)
        {
            PlayerPrefs.SetInt("IsAchieved" + i, isAchievedValue[i]);
        }

        // Сохранение статистики
        PlayerPrefs.SetInt("CurrentDistance", currentDistance);
        PlayerPrefs.SetInt("CurrentScore", currentScore);
        PlayerPrefs.SetInt("CurrentBones", currentBones);
        PlayerPrefs.SetInt("CurrentGoldenBones", currentGoldenBones);
        PlayerPrefs.SetInt("CurrentPoops", currentPoops);

        PlayerPrefs.SetInt("TotalDistance", totalDistance);
        PlayerPrefs.SetInt("TotalScore", totalScore);
        PlayerPrefs.SetInt("TotalBones", totalBones);
        PlayerPrefs.SetInt("TotalGoldenBones", totalGoldenBones);
        PlayerPrefs.SetInt("TotalPoops", totalPoops);
    }

    #endregion
}
