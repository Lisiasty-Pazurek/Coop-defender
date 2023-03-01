using UnityEngine;
using Mirror;
using System;

public class PlayerScore : NetworkBehaviour
    {
    [SyncVar] public int index;

    [SyncVar (hook = nameof(ScoreChange))] public int score;
    public UIHandler uiHandler;

    public override void OnStartLocalPlayer()
    {
        uiHandler = FindObjectOfType<UIHandler>();
    }


    public void ScoreChange(int lastScore, int currentScore)
    {
        if (!isLocalPlayer) {return;}
        ChangeUIScore();
    }
    

    void ChangeUIScore()
    {
        if (!isLocalPlayer) {return;}
        uiHandler.ChangeScore(score.ToString());
    }

}

