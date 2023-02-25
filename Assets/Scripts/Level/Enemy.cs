using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mirror;

public class Enemy : NetworkBehaviour
{
    [SerializeField] [SyncVar] public int enemyHealth = 3;

    // Homemade statemachine
    public bool isAlive;
    public bool isChasing;
    public bool isShooting;

    public NavMeshAgent enemyNavigator;
    public GameObject enemyTarget = null;
    [SerializeField] GameObject defensePoint = null;

    public override void OnStartServer()
    {
        base.OnStartServer();
        enemyNavigator = gameObject.GetComponent<NavMeshAgent>();
        isAlive = true;
        AsssignTarget();
    }

    private void Update ()
    {
        MoveToTarget();
    }

    [Server]
    public void AsssignTarget ()
    {
        // it will always find 1 palyer in hierarchy, that's a bug but have to keep it for now
        enemyTarget = GameObject.FindGameObjectWithTag("Player");
//        if ( enemyTarget != player)
//        { enemyTarget = defensePoint}
    }    
    
    [Server]
    public void MoveToTarget ()
    {
        // using navmesh for basic enemy movement, with changing enemyTarget it can be used for chasing/patrolling/following 
        if (enemyNavigator.destination == null) {return;}
        enemyNavigator.SetDestination(enemyTarget.transform.position);
        isChasing = true;

        float distance = Vector3.Distance(this.transform.position,enemyTarget.transform.position);
        // with setting up navmesh agent let enemy start shooting his target
        if (distance < 10)
        {
             isChasing = false;
             startShooting();
        }

    }

    [Server]
    public void startShooting ()
    {
        isShooting = true;
        // CmdShoot() using bullet prefab
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
        if (enemyHealth <= 0)
        // last hit for scoring point

        // player.GetComponent<PlayerScore>().score += 1;
        NetworkServer.Destroy(gameObject);
        //  NetworkServer.Spawn(deadbody); -- let people see some bloody massacre 
    }

}
