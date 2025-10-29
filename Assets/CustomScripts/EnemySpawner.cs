using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class EnemySpawner : MonoBehaviour
{
    public EnemyCharacter[] enemiesToSpawn;
    private List<EnemyCharacter> spawnedEnemies = new List<EnemyCharacter>();
    public float maxNumberOfSpawns;
    public float stopWatch;

    public bool isSpawnerEnabled=true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
 
    }

    // Update is called once per frame
    void Update()
    {
        if (isSpawnerEnabled)
        {
            if (spawnedEnemies.Count < maxNumberOfSpawns)
            {
                stopWatch += Time.deltaTime;
            }

            if (stopWatch >= 2.5f)
            {
                CleanOutDeadEnemies();
                SpawnEnemy();
                stopWatch = 0;
            }
        }
    }

    public void SpawnEnemy()
    {
        if (spawnedEnemies.Count < maxNumberOfSpawns)
        {
            if (enemiesToSpawn.Length > 0)
            {
                float RandomX = Random.Range(-4,4);
                float RandomZ = Random.Range(-4, 4);

                int index = Random.Range(0, enemiesToSpawn.Length);
                GameObject newEnemy = Instantiate(enemiesToSpawn[index].gameObject,transform.position+new Vector3(RandomX,0,RandomZ),Quaternion.identity);
                spawnedEnemies.Add(newEnemy.GetComponent<EnemyCharacter>());
            }

            if (spawnedEnemies.Count >= maxNumberOfSpawns)
            {
                stopWatch = 0;
            }
        }
    }

    public void CleanOutDeadEnemies()
    {
        if (spawnedEnemies.Count > 0)
        {
            for (int i = spawnedEnemies.Count - 1; i > -1; i--)
            {
                if (spawnedEnemies[i] == null || spawnedEnemies[i].isDead)
                {
                    spawnedEnemies.RemoveAt(i);
                }
            }
        }
    }
}
