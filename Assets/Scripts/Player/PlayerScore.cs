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
        uiHandler = this.GetComponentInParent<PlayerMovementController>().uiHandler;
    }

    [ServerCallback]
    public void ScoreChange(int oldValue, int newValue)
    {
        if (!isLocalPlayer) {return;}
        ChangeUIScore();
    }
    

    void ChangeUIScore()
    {
        uiHandler.ChangeScore(score.ToString());
    }

}

