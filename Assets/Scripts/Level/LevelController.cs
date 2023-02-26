using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MirrorBasics {
   



     
[RequireComponent (typeof (GameMode))]
public class LevelController : NetworkBehaviour
{   
    [SyncVar]  public List<PlayerMovementController> players = new List<PlayerMovementController> ();
    private MatchMaker matchMaker;
  
    private NetworkManager networkManager;
        
    [SyncVar] public Match currentMatch;

    [SyncVar] public string levelMatchID;

    [SyncVar] public bool readyToStart;

    public GameMode gameMode; 

    public bool readyToStartLevel;
    public bool countdownStarted = false;
    [SerializeField] private float countdownDuration = 1f;

        readonly public List<Match> levelmatches = new List<Match>();
        readonly public List<Player> matchPlayers = new List<Player>();
        readonly public List<PlayerMovementController> gamePlayers = new List<PlayerMovementController>();

        readonly public List<GameObject> spawnedItems = new List<GameObject>();

        [SerializeField] GameObject playerPrefab;

        public List<Transform> playerSpawnPoints = new List<Transform> ();
        private ArrayList spawnPoints;

        public bool gameEnded = false;

        private Scene onlineScene;
        private PlayerMovementController pController;

        public void Start() {  }

        public override void OnStartClient() 
        {
            UIGameplay uIGameplay = FindObjectOfType<UIGameplay>();
            // if (!CompareMatchId()) {return;} // it will be necessary for multiple spawned levels on server
            // else 
 //           uIGameplay.levelController = this;
            Player.localPlayer.levelController = this;
            gameMode = this.GetComponent<GameMode>();
        }

        public override void OnStartLocalPlayer()   
        { 

        }

        public override void OnStartServer() 
        {
            gameMode = this.GetComponent<GameMode>();
        }

    [Server]
    public void InitiateLevel(string levelMatchID)
    {
        Debug.Log("Level Controller starting for match: " + levelMatchID);
        matchMaker = GameObject.FindObjectOfType<MatchMaker>();
        networkManager = GameObject.FindObjectOfType<NetworkManager>();

        for (int i = 0; i < matchMaker.matches.Count; i++) {
            if (matchMaker.matches[i].matchID == levelMatchID) 
            {
                levelmatches.Add(matchMaker.matches[i]);
                Debug.Log("Passing match list from matchmaker to levelcontroller");
            }
        }
        for (int i = 0; i < matchMaker.matches.Count;i++)
        {   
            if (matchMaker.matches[i].matchID == levelMatchID) 
            {
                currentMatch = matchMaker.matches[i];
                matchPlayers.AddRange(currentMatch.players);
                Debug.Log("For levelMatch: " + levelMatchID +" currentMatch.matchID = " + currentMatch.matchID + " and : " + matchMaker.matches[i].matchID + " what is i: " + i + " Amount of players in this match:  "+ currentMatch.players.Count);
            }
            
        }
        CheckIfMatchPlayersAreReady();
    }

    public void CheckIfMatchPlayersAreReady()
    {
        if (readyToStart){return;}
        int k = 0; 
        foreach (Player player in matchPlayers) 
        {
            if (player.isReady == true)
            {k++;}
        }
        if (k == matchPlayers.Count)  {readyToStart = true;}
        // CheckifLevelisReadyToStart(readyToStart);
        PrepareLevel(levelMatchID);
        
    }


    [Server]
    public void PrepareLevel(string levelMatchID)
    {
        int playersAmount = matchPlayers.Count;
        Debug.Log("Players in game: " + playersAmount);
        GetPlayerSpawnPoints();
        SpawnPlayers(levelMatchID);
    }

// ### Disabled for debugging + setting up/moving to other script as a server doesnt have access to loaded level 

    // private void GetPlayerSpawnPoints(string spawnType)
    // {
    //     GameObject[] spawnPoints;
    //     spawnPoints = GameObject.FindGameObjectsWithTag(spawnType);
    //     foreach (GameObject spawnPoint in spawnPoints)  
    //     { playerSpawnPoints.Add(spawnPoint.transform);}
    //     Debug.Log("Ended getting PlayerSpawnPoints");
    // }

    private void GetPlayerSpawnPoints()
    {
        int t = 0;
        foreach (Player player in matchPlayers)
        {
            playerSpawnPoints.Add(GameObject.FindGameObjectsWithTag("PlayerSpawnPoint")[t].transform);
            t++;
        }
        Debug.Log("Ended getting PlayerSpawnPoints");
    
    }

    [Server]
    public void SpawnPlayers (string levelMatchID) 
    {
        Debug.Log("SpawnPlayers function: Attempting to spawn players");
           
                int t = 0;
                
                foreach (var player in matchPlayers) 
                {   
                    // Logic works for 2 teams, it has to be changed for multi team gamemode and new spawning method
                    if (player.matchID != levelMatchID) {return;} 

                   
                    GameObject go = Instantiate(playerPrefab, playerSpawnPoints[t]);
;
                    go.GetComponent<NetworkMatch>().matchId = player.GetComponent<NetworkMatch>().matchId;

                    NetworkServer.ReplacePlayerForConnection(player.connectionToClient, go, true);
                    gamePlayers.Add(go.GetComponent<PlayerMovementController>());
                    NetworkServer.SetClientReady(gamePlayers[t].connectionToClient);

                    Debug.Log("SpawnPlayers function: moved player to gamePlayer list");
                    gamePlayers[t].GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;

                    t++;

                 }

    }


    public static bool IsOdd(int value)
    {
        return value % 2 != 0;
    }

[Server]
    public void CheckIfGamePlayersAreReady()
    {
        int k = 0; 
        foreach (PlayerMovementController gamePlayer in gamePlayers) 
        {
            if (gamePlayer.isReady == true)
            k++;
            Debug.Log("checking if gameplayer ready: " + gamePlayer.netId + " is ready: " + gamePlayer.isReady);
        }

        if (k == gamePlayers.Count)  {readyToStartLevel = true;}
        Debug.Log(" [2] gamePlayers amount: " + gamePlayers.Count + " loop of: " + k + " is game ready to start? " + readyToStartLevel);

        if (!readyToStartLevel || countdownStarted){ return;} 
        else { Countdown(); }
    }

    [Server]
    private void Countdown()
    { 
        countdownStarted = true;
        float timeLeft = countdownDuration;
        while (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            Debug.Log(" Countdown for " + timeLeft);
            
        }
        Debug.Log("Ending Countdown for " + levelMatchID);
        SetGamePlayersReady();
    }

    [Server]
    private void SetGamePlayersReady()
    {

        foreach (PlayerMovementController gamePlayer in gamePlayers) 
            {
                gamePlayer.isReady = true;
                Debug.Log("Final setting levelcontroller to ready gamePlayerof id: " +gamePlayer.netId );
            }
    }

    [ClientRpc]
    public void EndLevel()
    {
        Debug.Log("Ending level for match: " + levelMatchID);
        ClientLeaveMatch();
        CleanSpawnedObjects();
    }

    [Client]
    private void ClientLeaveMatch() 
    {
        Player.localPlayer.currentMatch = null;
        Player.localPlayer.UnloadClientScene(gameMode.mapName);
//        Player.localPlayer.uIGameplay.ChangeUIState(3);        
    }

    [Server]
    public void CleanSpawnedObjects()
    {
        foreach (GameObject item in spawnedItems)
        {
            if (item != null) 
            Destroy(item);
        }
    }
    
    public bool CompareMatchId ()
    {
        if (this.currentMatch.matchID == NetworkClient.connection.identity.GetComponent<Player>().matchID)
        { return true;}
        else return false;
    }
}


}


