using System;
using System.Collections.Generic;
using Steamworks;
using Steamworks.Data;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameLobbyUIHandler : MonoBehaviour
{

    private GameNetworkManager gnmInstance;

    public Button startGameButton;
    public TextMeshProUGUI roomDetailsText;
    public List<TextMeshProUGUI> players;
    public GameObject playerNamePrefab;
    public Transform playerNameHolder;

    private int maxPlayerCount = 0;
    private int joinedPlayerCount = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gnmInstance = GameNetworkManager.Instance;

        gnmInstance.LobbyDataChanged += OnLobbyDataChanged;
        gnmInstance.LobbyMemberJoined += OnLobbyMemberJoined;

        GameObject meInstance = Instantiate(playerNamePrefab, playerNameHolder);
        TextMeshProUGUI meText = meInstance.GetComponent<TextMeshProUGUI>();

        meText.text = gnmInstance.CurrentUser.Name;

        startGameButton.onClick.AddListener(OnStartGameClick);
    }

    void OnDestroy()
    {
        gnmInstance.LobbyDataChanged -= OnLobbyDataChanged;
        gnmInstance.LobbyMemberJoined -= OnLobbyMemberJoined;
    }

    private void OnLobbyMemberJoined(Lobby lobby, Friend friend)
    {

        GameObject playerEntry = Instantiate(playerNamePrefab, playerNameHolder);
        TextMeshProUGUI playerText = playerEntry.GetComponent<TextMeshProUGUI>();
        playerText.text = friend.Name;

        roomDetailsText.text = $"{lobby.GetData("name")} ({lobby.MemberCount}/{lobby.MaxMembers})";

        // foreach (Friend player in lobby.Members)
        // {

        //     players.Add(playerText);
        // }
    }

    private void OnLobbyDataChanged(Lobby lobby)
    {
        roomDetailsText.text = $"{lobby.GetData("name")} ({lobby.MemberCount}/{lobby.MaxMembers})";
    }

    private void OnStartGameClick()
    {
        Debug.Log("game started");
        if (!NetworkManager.Singleton.IsHost)
        {
            return;
        }

        NetworkManager.Singleton.SceneManager.LoadScene("Main", LoadSceneMode.Single);
    }

    // Update is called once per frame
    void Update()
    {
        // joinedPlayerCount = gnmInstance.CurrentLobby.Value.MemberCount;
    }
}
