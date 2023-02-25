using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Bullet : NetworkBehaviour
{
    [SerializeField] public float bulletSpeed = 100f;
    [SerializeField] private float bulletLifetime = 3f;
    [SerializeField] private int damageAmount = 1;
    [SerializeField] private bool canDamagePlayer = false;

    private float bulletTimer = 0f;

    public override void OnStartServer()
    {
        base.OnStartServer();
        // Set bullet timer to zero when bullet is spawned
        bulletTimer = 0f;

        // Bullet ownership reference for scoring a point and any other usual stuff
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
    private void OnTriggerEnter(Collider other)
    {
        // Check if bullet collided with a player
        PlayerController player = other.gameObject.GetComponent<PlayerController>();
        if (canDamagePlayer && player != null)
        {
            // Apply damage to the player 
            player.TakeDamage(damageAmount);
        }
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            // Apply damage to the enemy 
             enemy.TakeDamage(damageAmount);
        }

        NetworkServer.Destroy(gameObject);
    }
}