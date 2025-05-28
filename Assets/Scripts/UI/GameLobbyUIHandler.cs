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
    [Header("UI References")]
    public Button startGameButton;
    public TextMeshProUGUI roomDetailsText;
    public GameObject playerNamePrefab;
    public Transform playerNameHolder;

    private GameNetworkManager gnmInstance;
    private readonly List<GameObject> playerEntries = new List<GameObject>();

    #region Unity Lifecycle

    void Start()
    {
        InitializeNetworkManager();
        SubscribeToEvents();
        SetupCurrentUser();
        InitializeLobbyDisplay();
        SetupStartButton();
    }

    void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    #endregion

    #region Initialization

    private void InitializeNetworkManager()
    {
        gnmInstance = GameNetworkManager.Instance;
        
        if (gnmInstance == null)
        {
            Debug.LogError("GameNetworkManager.Instance is null!");
        }
    }

    private void SubscribeToEvents()
    {
        if (gnmInstance == null) return;

        gnmInstance.LobbyDataChanged += OnLobbyDataChanged;
        gnmInstance.LobbyMemberJoined += OnLobbyMemberJoined;
        gnmInstance.LobbyEntered += OnLobbyEntered;
    }

    private void UnsubscribeFromEvents()
    {
        if (gnmInstance == null) return;

        gnmInstance.LobbyDataChanged -= OnLobbyDataChanged;
        gnmInstance.LobbyMemberJoined -= OnLobbyMemberJoined;
        gnmInstance.LobbyEntered -= OnLobbyEntered;
    }

    private void SetupCurrentUser()
    {
        if (gnmInstance?.CurrentUser.Id.IsValid == true)
        {
            // Don't create user entry here - it will be created in PopulatePlayerList
            // This prevents duplicate entries
        }
    }

    private void InitializeLobbyDisplay()
    {
        if (gnmInstance?.CurrentLobby.HasValue == true)
        {
            UpdateLobbyDisplay(gnmInstance.CurrentLobby.Value);
        }
    }

    private void SetupStartButton()
    {
        startGameButton?.onClick.AddListener(OnStartGameClick);
    }

    #endregion

    #region Event Handlers

    private void OnLobbyEntered(Lobby lobby)
    {
        UpdateLobbyDisplay(lobby);
    }

    private void OnLobbyMemberJoined(Lobby lobby, Friend friend)
    {
        UpdateLobbyDisplay(lobby);
    }

    private void OnLobbyDataChanged(Lobby lobby)
    {
        UpdateLobbyDisplay(lobby);
    }

    private void OnStartGameClick()
    {
        if (!NetworkManager.Singleton.IsHost)
        {
            Debug.LogWarning("Only the host can start the game.");
            return;
        }

        Debug.Log("Starting game...");
        NetworkManager.Singleton.SceneManager.LoadScene("Main", LoadSceneMode.Single);
    }

    #endregion

    #region UI Updates

    private void UpdateLobbyDisplay(Lobby lobby)
    {
        UpdateRoomDetails(lobby);
        RefreshPlayerList(lobby);
    }

    private void UpdateRoomDetails(Lobby lobby)
    {
        string lobbyName = lobby.GetData("name");
        roomDetailsText.text = $"{lobbyName} ({lobby.MemberCount}/{lobby.MaxMembers})";
    }

    private void RefreshPlayerList(Lobby lobby)
    {
        ClearPlayerList();
        PopulatePlayerList(lobby);
    }

    private void ClearPlayerList()
    {
        foreach (GameObject entry in playerEntries)
        {
            if (entry != null)
            {
                Destroy(entry);
            }
        }
        playerEntries.Clear();
    }

    private void PopulatePlayerList(Lobby lobby)
    {
        // Create a list to sort members - host first, then others
        var sortedMembers = new List<Friend>();
        
        // Add host first
        if (lobby.Owner.Id.IsValid)
        {
            sortedMembers.Add(lobby.Owner);
        }
        
        // Add other members (excluding host to avoid duplicates)
        foreach (Friend member in lobby.Members)
        {
            if (member.Id != lobby.Owner.Id)
            {
                sortedMembers.Add(member);
            }
        }
        
        // Create UI entries in order
        foreach (Friend member in sortedMembers)
        {
            bool isCurrentUser = member.Id == gnmInstance.CurrentUser.Id;
            bool isHost = member.Id == lobby.Owner.Id;
            CreatePlayerEntry(member.Name, isCurrentUser, isHost);
        }
    }

    private void CreatePlayerEntry(string playerName, bool isCurrentUser = false, bool isHost = false)
    {
        if (playerNamePrefab == null || playerNameHolder == null)
        {
            Debug.LogError("Player name prefab or holder is not assigned!");
            return;
        }

        GameObject playerEntry = Instantiate(playerNamePrefab, playerNameHolder);
        TextMeshProUGUI playerText = playerEntry.GetComponent<TextMeshProUGUI>();
        
        if (playerText != null)
        {
            // Add host indicator and format text
            string displayName = playerName;
            if (isHost)
            {
                displayName = $"👑 {playerName} (Host)";
            }
            
            playerText.text = displayName;
            
            // Optional: Add visual distinction for current user
            if (isCurrentUser)
            {
                playerText.fontStyle = FontStyles.Bold;
            }
        }

        playerEntries.Add(playerEntry);
    }

    #endregion
}