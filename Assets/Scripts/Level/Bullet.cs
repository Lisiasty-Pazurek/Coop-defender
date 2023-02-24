using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Bullet : NetworkBehaviour
{
    [SerializeField] public float bulletSpeed = 100f;
    [SerializeField] private float bulletLifetime = 3f;

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
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if bullet collided with a player
        PlayerController player = other.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            // Apply damage to the player 
            player.TakeDamage(1);
        }
        // Enemy enemy = other.GetComponent<Enemy>();
        // if (enemy != null)
        // {
        //     // Apply damage to the enemy 
        //     enemy.TakeDamage(1);
        // }

        Destroy(gameObject);
    }
}