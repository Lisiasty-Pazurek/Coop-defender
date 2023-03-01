using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Health : NetworkBehaviour
{
    [Header ("Attributes")]
    [SerializeField] [SyncVar (hook = nameof(ChangeHealth))] public int playerHealth = 5;
    [SyncVar]public bool isAlive = false;   

    [Header ("References")]
    public UIHandler uiHandler;

    // public override void OnStartServer ()
    // {
    //     uiHandler = FindObjectOfType<UIHandler>();
    // }
    public override void OnStartLocalPlayer ()
    {
        uiHandler = FindObjectOfType<UIHandler>();
    }

    public void TakeDamage(int damageAmount)
    {
        if (!isAlive || !isLocalPlayer) {return;}
        playerHealth -= damageAmount;
        if (playerHealth <= 0)
        {   
            StartCoroutine(PlayerWounded());
        }
        Debug.Log("Got hit! " + netId);
    }

    // Death/healing/respawn -- maybe some additional ressurecting interaction

    public IEnumerator PlayerWounded()
    {
        if (!isLocalPlayer) {yield break;}
        isAlive = false;
        Debug.Log("Need a medic! " + netId);
        // need some visual feedback with animator but works as intended
        while (isAlive == false)
        {
            yield return new WaitForSeconds(8f); 
            
            isAlive = true;
        }
        playerHealth = 5;
    }

// not working as it should on client
    public void ChangeHealth(int lastHP, int currentHP)
    {
        if (!isLocalPlayer) {return;}
        ChangeUIHealth();
    }


    void ChangeUIHealth()
    {
//        if (!isLocalPlayer) {return;}
        uiHandler.ChangeHealth(playerHealth.ToString());
    }

}
