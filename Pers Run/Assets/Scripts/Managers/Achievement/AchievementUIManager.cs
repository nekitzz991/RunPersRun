using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AchievementUIManager : MonoBehaviour
{
    [Header("Настройки UI")]
    [SerializeField] private Transform achievementContainer; // Контейнер для карточек достижений
    [SerializeField] private GameObject achievementPrefab; // Префаб карточки достижения

    private Dictionary<string, GameObject> achievementUIElements = new Dictionary<string, GameObject>();

    private void Start()
    {
        if (AchievementManager.Instance != null)
        {
            PopulateAchievements();
            AchievementManager.OnAchievementUnlocked += UpdateAchievementUI;
        }
    }

    private void OnDestroy()
    {
        if (AchievementManager.Instance != null)
        {
            AchievementManager.OnAchievementUnlocked -= UpdateAchievementUI;
        }
    }

    /// <summary>
    /// Создаёт UI-карточки для всех достижений.
    /// </summary>
    private void PopulateAchievements()
    {
        foreach (var achievement in AchievementManager.Instance.achievementDataList)
        {
            CreateAchievementUI(achievement);
        }
    }

    /// <summary>
    /// Создаёт отдельную UI-карточку для достижения.
    /// </summary>
    private void CreateAchievementUI(AchievementData achievementData)
    {
        GameObject newAchievement = Instantiate(achievementPrefab, achievementContainer);
        achievementUIElements[achievementData.id] = newAchievement;

        Text nameText = newAchievement.transform.Find("NameText").GetComponent<Text>();
        Text descriptionText = newAchievement.transform.Find("DescriptionText").GetComponent<Text>();
        Image icon = newAchievement.transform.Find("AchievementIcon").GetComponent<Image>();
        GameObject lockIcon = newAchievement.transform.Find("LockIcon").gameObject;

        nameText.text = achievementData.achievementName;
        descriptionText.text = achievementData.description;
        icon.sprite = achievementData.isUnlocked ? achievementData.unlockedSprite : achievementData.lockedSprite;

        // Если достижение скрытое и не разблокировано, маскируем его
        if (achievementData.isSecret && !achievementData.isUnlocked)
        {
            nameText.text = "???";
            descriptionText.text = "Скрытое достижение";
            lockIcon.SetActive(true);
        }
        else
        {
            lockIcon.SetActive(false);
        }
    }

    /// <summary>
    /// Обновляет UI при разблокировке достижения.
    /// </summary>
    private void UpdateAchievementUI(AchievementData achievementData)
    {
        if (achievementUIElements.TryGetValue(achievementData.id, out GameObject achievementUI))
        {
            achievementUI.transform.Find("AchievementIcon").GetComponent<Image>().sprite = achievementData.unlockedSprite;

            Text nameText = achievementUI.transform.Find("NameText").GetComponent<Text>();
            Text descriptionText = achievementUI.transform.Find("DescriptionText").GetComponent<Text>();
            GameObject lockIcon = achievementUI.transform.Find("LockIcon").gameObject;

            nameText.text = achievementData.achievementName;
            descriptionText.text = achievementData.description;
            lockIcon.SetActive(false);
        }
    }
}
