using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class EnemySpawner : NetworkBehaviour
{
    [SerializeField] public GameObject enemyPrefab;
    [SerializeField] public float spawnInterval;
    [SerializeField] public int  maxEnemies = 10;
    [SerializeField] [SyncVar]public int numEnemies = 0;
    [SerializeField] List<Transform> enemySpawnPoints;

    public GameSession session;



    // Start spawning enemies when the game starts
    public override void OnStartServer()
    {
        base.OnStartServer();
        StartCoroutine(SpawnEnemyCoroutine());
        session = FindObjectOfType<GameSession>();
    }

    private void Update ()
    {
        numEnemies = GameObject.FindObjectsOfType<Enemy>().Count();
    }

    [Server]
    // Coroutine ends after spawning 10th enemy so need to be restarted once 10th enemy dies
    private IEnumerator SpawnEnemyCoroutine()
    {
        
        while (numEnemies < maxEnemies)
        {

            // Wait for the spawn interval
            yield return new WaitForSeconds(spawnInterval);

            // Spawn an enemy if we haven't reached the max yet
            if (numEnemies < maxEnemies)
            {   
                if (session.gameEnded) {yield break;}
                GameObject enemy = Instantiate(enemyPrefab);
                NetworkServer.Spawn(enemy);
                enemy.transform.position = enemySpawnPoints[Random.Range(0, enemySpawnPoints.Count)].position;
            }
       
        }

    }
}


