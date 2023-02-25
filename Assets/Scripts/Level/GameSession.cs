using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using System;

public class GameSession : NetworkBehaviour
{
    [Header("Game Settings")]
    public float gameDuration = 180f;
    public float countdownDuration = 3f;

    [Header("UI Elements")]
    public Text countdownText;
    public Text gametimeText;
    public Text scoreText;

    [Header("Prefabs")]
    // public GameObject playerPrefab;

    [SyncVar(hook = nameof(OnScoreChanged))]
    private int score;

    private float countdownTimer;
    private float gameTimer;
    private bool gameEnded;

    public override void OnStartServer()
    {
        base.OnStartServer();

        // Spawn player prefabs for each connected client
        NetworkServer.SpawnObjects();

        // Start the countdown
        countdownTimer = countdownDuration;
        gameTimer = gameDuration;
        gameEnded = false;
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
//                RpcEnablePlayerController();
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
        countdownText.text = "Game starting in " + secondsLeft + " seconds...";
    }

    [ClientRpc]
    private void RpcUpdateGameTimer(int secondsLeft)
    {
        gametimeText.text =  secondsLeft + " seconds";
    }

    [ClientRpc]
    private void RpcDisableCountdown()
    {
        countdownText.gameObject.SetActive(false);
    }

    // [Command]
    // public void CmdAddScore(int points)
    // {
    //     score += points;
    // }

    private void OnScoreChanged(int oldScore, int newScore)
    {
        scoreText.text = "Score: " + newScore;
    }

    private void EndGame()
    {
        gameEnded = true;

        Debug.Log("Game ended");
        // Display scoreboard
        // ...
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