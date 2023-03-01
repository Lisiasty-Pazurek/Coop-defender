using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour
{
    [SerializeField] public Text healthText;
    [SerializeField] public Text scoreText;    


    public void ChangeHealth(string hpAmount)
    {
        healthText.text = hpAmount;
    }

    public void ChangeScore (string scoreAmount)
    {
        scoreText.text = scoreAmount;
    }

    public void ChangeCountdown()
    {
//        countdown.text = countText;
    }

    public void RestartLevel ()
    {
        FindObjectOfType<NetworkRoomManagerExt>().ServerChangeScene(FindObjectOfType<NetworkRoomManagerExt>().RoomScene);
    } 
}
