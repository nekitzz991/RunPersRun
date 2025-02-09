using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelDestroy : MonoBehaviour
{
    private LevelGenerator levelGenterator;
    private PersRunner player;

    // Start is called before the first frame update
    void Start()
    {
        levelGenterator = FindObjectOfType<LevelGenerator>();
        player = FindObjectOfType<PersRunner>();
    }

    // Update is called once per frame
    void Update()
    {
        DeleteFinalParts();
    }

    private void DeleteFinalParts()
    {
        if(transform.position.x < player.transform.position.x - 100)
        {
            Destroy(gameObject);
        }
    }
}
