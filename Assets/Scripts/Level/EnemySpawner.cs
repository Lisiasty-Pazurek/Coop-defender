using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;


namespace MirrorBasics {
public class EnemySpawner : NetworkBehaviour
{
    [SerializeField] public GameObject enemyPrefab;
    [SerializeField] public float spawnInterval;
    [SerializeField] public int  maxEnemies = 10;
    [SerializeField] [SyncVar]public int numEnemies = 0;
    [SerializeField] List<Transform> enemySpawnPoints;
    LevelController levelController;

    public GameSession session;



    // Start spawning enemies when the game starts
    public override void OnStartServer()
    {
        levelController = FindObjectOfType<LevelController>();
        session = FindObjectOfType<GameSession>();
        StartCoroutine(SpawnEnemyCoroutine());
    }

    private void Update ()
    {
      
        numEnemies = GameObject.FindObjectsOfType<Enemy>().Count();
    }

    // Coroutine ends after spawning 10th enemy so need to be restarted once 10th enemy dies
    [Server]
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
                GameObject enemy = Instantiate(enemyPrefab,enemySpawnPoints[Random.Range(0, enemySpawnPoints.Count)]);
                enemy.GetComponent<NetworkMatch>().matchId =  levelController.GetComponent<NetworkMatch>().matchId;
                NetworkServer.Spawn(enemy);
                
            }
       
        }

    }
}
}


