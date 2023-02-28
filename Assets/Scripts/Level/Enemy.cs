using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mirror;

public class Enemy : NetworkBehaviour
{
    [Header ("Settings")]
    [SerializeField] [SyncVar] public int enemyHealth = 3;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform bulletSpawnPoint;
    [SerializeField] public float reloadCD = 1;

    [Header("State")]
    // Homemade statemachine
    public bool isAlive;
    public bool isChasing;
    public bool isShooting;


    public float reloadTime;

    [Header ("References")]
    public NavMeshAgent enemyNavigator;
    public GameObject enemyTarget = null;
    [SerializeField] GameObject defensePoint = null;
    private EnemySpawner enemySpawner;
    private GameObject lastShooter;

    public override void OnStartServer()
    {
        base.OnStartServer();
        enemySpawner = GameObject.FindObjectOfType<EnemySpawner>();
        enemyNavigator = gameObject.GetComponent<NavMeshAgent>();
        defensePoint = GameObject.FindGameObjectWithTag("defensePoint");
        isAlive = true;
        AsssignTarget();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
    }

    [ServerCallback]
    private void Update ()
    {
        if (!isAlive) {return;}
        MoveToTarget();
        reloadCD -= Time.deltaTime;
    }

    [Server]
    public void AsssignTarget ()
    {
        // it will always find 1 player in hierarchy, good enough if there is only one connected player
        enemyTarget = GameObject.FindGameObjectWithTag("Player");

        // looking for workaround to follow player that is closer
        if (NetworkServer.connections.Count > 1)
        {
            foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
            {
                float distance = Vector3.Distance(this.transform.position, player.transform.position);
            }
        }
        
        // Players are dead, waiting for respawn, or too far away, also.. can be swapped to additonal trigger collider for checking again
 //       if (enemyTarget = null) { enemyTarget = defensePoint;}
    }    
    
    [ServerCallback]
    public void MoveToTarget ()
    {
        if (!isAlive) { enemyNavigator.ResetPath(); }
        // using navmesh for basic enemy movement, with changing enemyTarget it can be used for chasing/patrolling/following 
        if (enemyNavigator.destination == null) {return;}

        float distance = Vector3.Distance(this.transform.position, enemyTarget.transform.position);  
        
        if (distance >12)
        {    
            isChasing = true;      
            enemyNavigator.SetDestination(enemyTarget.transform.position);
        }

        // with setting up navmesh agent let enemy start shooting his target
        if (4 < distance && distance < 12)
        {
            isChasing = false;
            startShooting();
        }



    }

    [Server]
    public void startShooting ()
    {
        isShooting = true;
        StartCoroutine(Shooting());
    }

    [Server]
    public IEnumerator Shooting ()
    {
        float distance = Vector3.Distance(this.transform.position,enemyTarget.transform.position);
        while (distance < 12 && distance > 4)
        {

            // Wait for the reload
            yield return new WaitForSeconds(reloadCD);
            if (reloadCD < 0)
            {
                EnemyFire();
                reloadCD = reloadTime ;
            }
        }
        isShooting = false;
    }

    [Server]
    public void EnemyFire()
    {
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        // !!!! Spawn bullet on clients - BEFORE APPLYING FORCE     
        NetworkServer.Spawn(bullet);
        // Apply force to bullet in the direction the player is facing
        bullet.GetComponent<Bullet>().shooter = this.gameObject;
        bullet.GetComponent<Rigidbody>().AddForce(transform.forward * bullet.GetComponent<Bullet>().bulletSpeed);

        // Spawn bullet on clients

    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        //Check if get hit by bullet
        if (other.GetComponent<Bullet>() != null)
        {
            Bullet bullet = other.GetComponent<Bullet>();
            if (bullet.canDamageEnemy)
            // Apply damage to enemy 
            TakeDamage(bullet.damageAmount);
            lastShooter = bullet.shooter;
        }
    }

    [Server]
    public void TakeDamage(int amount)
    {
        enemyHealth -= amount;
        EnemyDie();
    } 

    [Server]
    public void EnemyDie ()
    {
        if (enemyHealth <= 0) {
        isAlive = false;
        // last hit for scoring point, not giving score if shot by another enemy 
        if (lastShooter.GetComponent<PlayerScore>() != null) { lastShooter.GetComponent<PlayerScore>().score +=1;} 
        NetworkServer.Destroy(gameObject);
        //  NetworkServer.Spawn(deadbody); -- let people see some "positive feedback"", need some additional models/animations/else
        }
    }



    

}
