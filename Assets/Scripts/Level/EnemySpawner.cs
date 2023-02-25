using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class EnemySpawner : NetworkBehaviour
{
    [SerializeField] public GameObject enemyPrefab;
    [SerializeField] public float spawnInterval;
    [SerializeField] public int  maxEnemies = 10;
    private int numEnemies = 0;
    [SerializeField] List<Transform> enemySpawnPoints;



    // Start spawning enemies when the game starts
    public override void OnStartServer()
    {
        StartCoroutine(SpawnEnemyCoroutine());
    }

    private IEnumerator SpawnEnemyCoroutine()
    {
        while (numEnemies < maxEnemies)
        {
            // Wait for the spawn interval
            yield return new WaitForSeconds(spawnInterval);

            // Spawn an enemy if we haven't reached the max yet
            if (numEnemies < maxEnemies)
            {
                GameObject enemy = Instantiate(enemyPrefab,enemySpawnPoints[Random.Range(0, enemySpawnPoints.Count)]);
                NetworkServer.Spawn(enemy);
                numEnemies++;
            }
        }
    }
}


