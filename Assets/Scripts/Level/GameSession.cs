using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using System;


namespace MirrorBasics
{
public class GameSession : NetworkBehaviour
{
    [Header("Game Settings")]
    public float gameDuration = 180f;
    [SerializeField] public float countdownDuration = 3f;

    [Header("UI Elements")]
    public Text countdownText;
    public Text gametimeText;
    [SyncVar] private float countdownTimer;
    private float gameTimer;
    public bool gameEnded;


    public Canvas postGameWindow;

    [Header("References")]
    public PlayerMovementController playerController;
    [SyncVar]public Match currentMatch;
    LevelController levelController;



    public override void OnStartServer()
    {
        base.OnStartServer();

        // Spawn player prefabs for each connected client
        NetworkServer.SpawnObjects();

        // Start the countdown
        countdownTimer = countdownDuration;
        gameTimer = gameDuration;
        gameEnded = false;
        currentMatch = FindObjectOfType<LevelController>().currentMatch;
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        // playerController = 
        
    }

    [ServerCallback]
    private void Update()
    {
        if (gameEnded) return;

        // Countdown before game starts
        if (countdownTimer > 0f)
        {
            countdownTimer -= Time.deltaTime;
            int secondsLeft = Mathf.CeilToInt(countdownTimer);
            if (secondsLeft != Mathf.CeilToInt(countdownTimer))
            {
                RpcUpdateCountdown(secondsLeft);
            }

            if (countdownTimer <= 0f)
            {
                RpcDisableCountdown();
                RpcEnablePlayerController();
            }
        }
        else
        {
            // Game is in progress
            gameTimer -= Time.deltaTime;
            RpcUpdateGameTimer(Convert.ToInt32(gameTimer));
            if (gameTimer <= 0f)
            {
                EndGame();
            }
        }
    }

    [ClientRpc]
    private void RpcUpdateCountdown(int secondsLeft)
    {
        countdownText.text = secondsLeft.ToString();
        Debug.Log(" countdown" + secondsLeft);
    }

    [ClientRpc]
    private void RpcUpdateGameTimer(int secondsLeft)
    {
        gametimeText.text =  secondsLeft.ToString() ;
    }

    [ClientRpc]
    private void RpcDisableCountdown()
    {
        countdownText.gameObject.SetActive(false);
    }

    [ClientRpc]
    public void RpcEnablePlayerController()
    {
        playerController.isReady = true;
    }

    [Server]
    private void EndGame()
    {
        gameEnded = true;

        Debug.Log("Game ended");
        // Display scoreboard
        // RpcClient

        foreach (PlayerMovementController player in FindObjectsOfType<PlayerMovementController>())
        {
            player.isReady = false;
        }

        foreach (Enemy enemy in FindObjectsOfType<Enemy>())
        {
            enemy.isAlive = false;
        }

        postGameWindow.enabled = true;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        // Disable the countdown UI for non-local players
        if (!isLocalPlayer)
        {
            countdownText.gameObject.SetActive(false);
        }
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        // Destroy all player objects when the game ends
        if (gameEnded)
        {
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Player"))
            {
                Destroy(obj);
            }
        }
    }
}
}