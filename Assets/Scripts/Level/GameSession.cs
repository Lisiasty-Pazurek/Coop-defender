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
    [SerializeField] public float countdownDuration = 3;
    [SerializeField] public GameObject scorePrefab;

    [Header("UI Elements")]
    [SyncVar] public float countdownTimer;    
    public Text countdownText;
    public Text gametimeText;
    [SerializeField] public Transform scoreboardGrid;
    private float gameTimer;
    public bool gameEnded;

    public Canvas postGameWindow;

    [Header("References")]
    public PlayerMovementController playerController;
    
    public UIHandler uiHandler;


    public override void OnStartServer()
    {
        base.OnStartServer();

        // Spawn player prefabs for each connected client
//        NetworkServer.SpawnObjects(); // works without it, no need for overriding it

        // Start the countdown
        countdownTimer = countdownDuration;
        gameTimer = gameDuration;
        gameEnded = false;
        StartCoroutine(Countdown());
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        uiHandler = FindObjectOfType<UIHandler>();
        
    }

    [ServerCallback]
    private void Update()
    {
        if (gameEnded || countdownTimer >0) {return;}
            // Game is in progress
            gameTimer -= Time.deltaTime;

            RpcUpdateGameTimer(Convert.ToInt32(gameTimer));
            if (gameTimer <= 0f)
            {
                EndGame();
            }
        
    }

    [Server]
    public IEnumerator Countdown()
    {
        countdownText.gameObject.SetActive(true); // why is it disabling itself?!
        while (countdownTimer >= 0)
        {
            yield return new WaitForSecondsRealtime(1);
            int secondsLeft = Mathf.CeilToInt(countdownTimer);
            RpcUpdateCountdown(secondsLeft);
            countdownTimer-= 1;
        }

        if (countdownTimer < 0f)
        {
            RpcDisableCountdown();
            RpcEnablePlayerController(true);
            yield break;
        }
        
    }

    [ClientRpc]
    private void RpcUpdateCountdown(int secondsLeft)
    {
        countdownText.text = secondsLeft.ToString();
        
    }

    [ClientRpc]
    private void RpcUpdateGameTimer(int secondsLeft)
    {
        gametimeText.text =  secondsLeft.ToString() ;
    }

    [ClientRpc]
    private void RpcDisableCountdown()
    {
        countdownText.enabled = false;
    }

    [ClientRpc]
    public void RpcEnablePlayerController(bool state)
    {
        playerController.health.isAlive = state;     
    }

    [Server]
    private void EndGame()
    {
        gameEnded = true;

        Debug.Log("Game ended");
        // Display scoreboard

        foreach (Enemy enemy in FindObjectsOfType<Enemy>())
        {
            enemy.gameObject.SetActive(false);
        }

        RpcEndgame ();
        RpcEnablePlayerController(false);
        DisplayScore();
    }

    [Server]
    void DisplayScore()
    {
        Debug.Log("spawning score prefabs");
        foreach (PlayerScore player in FindObjectsOfType<PlayerScore>())
        {
            GameObject scorerow = Instantiate(scorePrefab, scoreboardGrid);            
            NetworkServer.Spawn(scorerow);
            scorerow.GetComponent<Text>().text = " Player id: "  + player.netId  + " score: " + player.score + "";

        }
    }

    [ClientRpc]
    public void RpcEndgame ()
    {

        postGameWindow.enabled = true;
        //need to add scoreboard here once connectivity issues will be fixed
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
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