using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    private const float PLAYER_DISTANCE_SPAWN_LEVEL_PART = 200f;

    [SerializeField] private int startingSpawnLevelParts ; 
    [SerializeField] private Transform levelPart_Start;
    [SerializeField] private List<Transform> levelPartList;

    private PersRunner player;
    private Vector3 lastEndPosition;

    private void Awake()
    {
        player = FindObjectOfType<PersRunner>();  
        lastEndPosition = levelPart_Start.Find("EndPoint").position;

        for (int i = 0; i < startingSpawnLevelParts; i++)
        {
            SpawnLevelPart();
        }
    }

   private void Start() 
{
    StartCoroutine(CheckSpawnCondition());
}

private IEnumerator CheckSpawnCondition()
{
    // Генерация начальных уровней
    for (int i = 0; i < startingSpawnLevelParts; i++)
    {
        SpawnLevelPart();
    }

    // Проверка генерации по мере движения игрока
    while (true)
    {
        if (Vector3.Distance(player.transform.position, lastEndPosition) < PLAYER_DISTANCE_SPAWN_LEVEL_PART)
        {
            SpawnLevelPart();
        }
        yield return new WaitForSeconds(0.2f); // Проверять 5 раз в секунду
    }
}


    private void SpawnLevelPart()
    {
        int randomIndex = Random.Range(0, levelPartList.Count);
        Transform chosenLevelPart = levelPartList[randomIndex];

        Transform newLevelPart = Instantiate(chosenLevelPart, lastEndPosition, Quaternion.identity);
        lastEndPosition = newLevelPart.Find("EndPoint").position;
    }
}
