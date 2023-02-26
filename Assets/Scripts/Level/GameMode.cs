using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MirrorBasics {
public class GameMode : MonoBehaviour
{
    public string gameModeName;
    public string mapName;
    public int minPlayers;
    public int maxPlayers;
    public int minTeams;
    public int maxTeams;
    public int maxScore;

    [SerializeField] public List<Transform> playerSpawnPoints;
    [SerializeField] public List<Transform> rewardSpawnPoints;
    [SerializeField] public List<Transform> teamSpawnPoints;
    [SerializeField] public List<Transform> pickupSpawnPoints;  
    public Scene gameScene;
    
    public void  Start() 
    {
        SetGameMode();
    }
    public void SetGameMode()
    {
        if (gameModeName == "Apples")
        {
            minPlayers = 2;
            maxPlayers = 10;
            minTeams = 2;
            maxTeams = 2;
            maxScore = 10;
            mapName = "Apples01";
        }
        if (gameModeName == "Quiz")
        {
            minPlayers = 1;
            maxPlayers = 20;
            minTeams = 1;
            maxTeams = 1;
            maxScore = 16;
            mapName = "Quiz01";
        }

        else return;
    }


}
}