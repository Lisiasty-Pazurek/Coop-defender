using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour
{
    [SerializeField] public Text healthText;
    [SerializeField] public Text scoreText;    


    public void ChangeHealth(string amount)
    {
        healthText.text = amount;
    }

    public void ChangeScore (string amount)
    {
        scoreText.text = amount;
    }

    public void RestartLevel ()
    {
        FindObjectOfType<NetworkRoomManagerExt>().ServerChangeScene(FindObjectOfType<NetworkRoomManagerExt>().RoomScene);
    } 
}
