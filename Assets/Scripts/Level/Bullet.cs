using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Bullet : NetworkBehaviour
{
    [SerializeField] private float bulletLifetime = 2f;    
    [SerializeField] public float bulletSpeed = 100f;
    [SerializeField] public int damageAmount = 1;

    // It can be useful for some friendly fire grenades etc.
    [SerializeField] public bool canDamagePlayer = false;
    [SerializeField] public bool canDamageEnemy = true;

    [SyncVar] public GameObject shooter;        // Bullet ownership reference for scoring a point and any other usual stuff

    private float bulletTimer = 0f;

    public override void OnStartServer()
    {
        base.OnStartServer();
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
            Destroy(gameObject);
        }
    }

    [ServerCallback]
    private void OnColliderEnter(Collider other)
    {
        // Destroy if hit something that's in way
        NetworkServer.Destroy(gameObject);
    }
}