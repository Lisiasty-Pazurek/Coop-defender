using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class LobbySystem : MonoBehaviour
{
    public NetworkRoomManagerExt networkManager;
    public LightReflectiveMirror.LightReflectiveMirrorTransport LRMTransport;

    [Header("Lobby Settings")]
    public bool autoRefeshServerList;
    public int refreshServerListTimer = 10;

    [Header("Lobby Menu")]
    public Transform LobbyMenuParent;
    public Text lobbyMenuText;
    public InputField joinRoomNameInputField;
    public Transform lobbyPanel;
    public Transform templateListing;
    public InputField playerNameInputField;

    [Header("Create A Room Menu")]
    public Transform createRoomMenuParent;
    public InputField roomNameInputField;
    public Slider maxPlayersSlider;
    public Text maxPlayersText;
    public Dropdown mapListDropdown;
    // Start is called before the first frame update
    void Awake()
    {
        roomNameInputField.text = "ROOM " + Random.Range(0, 999).ToString();
        playerNameInputField.text = "Player " + Random.Range(0, 999).ToString();
        Application.targetFrameRate = 60;
    }

    float listTimer = 0;
    private void Update()
    {
        if (autoRefeshServerList)
        {
            listTimer += 1 * Time.deltaTime;
            if(listTimer>= refreshServerListTimer)
            {
                listTimer = 0;
                if(!networkManager.isNetworkActive)RefreshServerList();
            }
        }

        maxPlayersText.text = "Maximum Players: " + maxPlayersSlider.value.ToString();
    }

    public void OpenLobbyMenu()
    {
        createRoomMenuParent.gameObject.SetActive(false);
        LobbyMenuParent.gameObject.SetActive(true);
    }

    public void OpenCreateRoomMenu()
    {
        createRoomMenuParent.gameObject.SetActive(true);
        LobbyMenuParent.gameObject.SetActive(false);
    }

    public void RefreshServerList()
    {
        LRMTransport.RequestServerList();
    }

    public void CreateRoom()
    {
        if(roomNameInputField.text.Length < 1) roomNameInputField.text = "ROOM " + Random.Range(0, 999).ToString();

        PlayerPrefs.SetString("PlayerName", playerNameInputField.text);

        LRMTransport.serverName = roomNameInputField.text;
        LRMTransport.maxServerPlayers = (int)maxPlayersSlider.value;
        LRMTransport.extraServerData = mapListDropdown.options[mapListDropdown.value].text;
        networkManager.StartHost();

        networkManager.ServerChangeScene(mapListDropdown.options[mapListDropdown.value].text);
    }

    public void JoinRoom()
    {
        LRMTransport.RequestServerList();

        for (int i = 0; i < LRMTransport.relayServerList.Count; i++)
        {
            if (LRMTransport.relayServerList[i].serverName.Normalize() == joinRoomNameInputField.text.Normalize())
            {
                string serverID = LRMTransport.relayServerList[i].serverId;
                NetworkManager.singleton.networkAddress = serverID.ToString(); NetworkManager.singleton.StartClient();
                return;
            }
        }
    }

    public void OnServerListUpdated()
    {
        foreach (Transform t in templateListing.parent)
            if(t.gameObject.activeSelf)Destroy(t.gameObject);

        int totalPlayers = 0;
        for (int i = 0; i < LRMTransport.relayServerList.Count; i++)
        {
            print(LRMTransport.relayServerList[i].relayInfo.address + " ADDRESS");

            Transform roomEntry = Instantiate(templateListing, templateListing.parent);

            roomEntry.Find("RoomText").GetComponent<Text>().text = LRMTransport.relayServerList[i].serverName;
            roomEntry.Find("DataText").GetComponent<Text>().text = LRMTransport.relayServerList[i].serverData;
            roomEntry.Find("PlayerText").GetComponent<Text>().text = LRMTransport.relayServerList[i].currentPlayers.ToString() + "/"+ LRMTransport.relayServerList[i].maxPlayers.ToString();
            string serverID = LRMTransport.relayServerList[i].serverId;
            roomEntry.Find("JoinButton").GetComponent<Button>().onClick.AddListener(() => { NetworkManager.singleton.networkAddress = serverID.ToString(); NetworkManager.singleton.StartClient(); });
            roomEntry.gameObject.SetActive(true);
            totalPlayers += LRMTransport.relayServerList[i].currentPlayers;
        }

        lobbyMenuText.text = "LOBBY - " + totalPlayers.ToString() + " PLAYERS IN ROOMS";

    }

    public void OnConnectedToRelay()
    {
        print("Connected to relay!");
        RefreshServerList();
    }

    public void OnDisconnectedFromRelay()
    {
        print("Disconnected from relay!");
    }
}
