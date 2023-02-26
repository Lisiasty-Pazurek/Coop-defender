using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Bullet : NetworkBehaviour
{
    [SerializeField] private float bulletLifetime = 2f;    
    [SerializeField] public float bulletSpeed = 100f;
    [SerializeField] public int damageAmount = 1;
    [SerializeField] public bool canDamagePlayer = false;
    [SerializeField] public bool canDamageEnemy = true;

    public GameObject shooter;        // Bullet ownership reference for scoring a point and any other usual stuff

    private float bulletTimer = 0f;

    public override void OnStartServer()
    {
        base.OnStartServer();
        this.GetComponent<NetworkMatch>().matchId = shooter.GetComponent<NetworkMatch>().matchId ;
        // Set bullet timer to zero when bullet is spawned
        bulletTimer = 0f;

    }

    private void FixedUpdate()
    {
        // Increment bullet timer
        bulletTimer += Time.deltaTime;

        // Destroy bullet after a certain amount of time
        if (bulletTimer >= bulletLifetime)
        {
            NetworkServer.Destroy(gameObject);
        }
    }

    [ServerCallback]
    private void OnColliderEnter(Collider other)
    {
        // Destroy if hit something that's in way
        NetworkServer.Destroy(gameObject);
    }
}