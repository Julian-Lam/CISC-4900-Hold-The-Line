using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;


public class EnemySpawner : MonoBehaviour
{
    public EnemyCharacter[] enemiesToSpawn;
    private List<EnemyCharacter> spawnedEnemies = new List<EnemyCharacter>();
    public float maxNumberOfSpawns;
    public float stopWatch;
    public Camera cam;

    public bool isSpawnerEnabled=true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (isSpawnerEnabled)
        {
            CleanOutDeadEnemies();

            if (spawnedEnemies.Count < maxNumberOfSpawns)
            {
                stopWatch += Time.deltaTime;
            }

            if (stopWatch >= 2.5f)
            {
                if (Vector3.Distance(transform.position,cam.transform.position)<=35)
                {
                    SpawnEnemy();
                }
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
                Vector3 spawncoords = GetNewSpawnCoordinates(4);

                while (!CheckIfCoordsAreNavMesh(spawncoords))
                {
                    spawncoords = GetNewSpawnCoordinates(4);
                }

                int index = Random.Range(0, enemiesToSpawn.Length);
                GameObject newEnemy = Instantiate(enemiesToSpawn[index].gameObject,transform.position+spawncoords,Quaternion.identity);
                spawnedEnemies.Add(newEnemy.GetComponent<EnemyCharacter>());
            }

            if (spawnedEnemies.Count >= maxNumberOfSpawns)
            {
                stopWatch = 0;
            }
        }
    }

    public static Vector3 GetNewSpawnCoordinates(float radius)
    {
        float RandomX = Random.Range(-radius, radius);
        float RandomZ = Random.Range(-radius, radius);

        return new Vector3(RandomX, 0, RandomZ);
    }

    public static bool CheckIfCoordsAreNavMesh(Vector3 coords)
    {
        if(NavMesh.SamplePosition(coords,out NavMeshHit hit, 1, NavMesh.AllAreas))
        {
            return true;
        }
        else
        {
            return false;
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
