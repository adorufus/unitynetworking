using UnityEngine;
using Steamworks;
using Steamworks.Data;
using Netcode.Transports.Facepunch;
using Unity.Netcode;
using Unity.VisualScripting;
using System;

public class GameNetworkManager : MonoBehaviour
{

    private FacepunchTransport transport;

    public static GameNetworkManager Instance { get; private set; } = null;
    public Lobby? CurrentLobby { get; set; } = null;

    public Friend CurrentUser { get; private set; }

    public Action<SteamId> OnHostCreated;
    public Action<Lobby> LobbyDataChanged;
    public Action<Lobby> LobbyEntered;
    public Action<Lobby, Friend> LobbyMemberJoined;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transport = NetworkManager.Singleton.GetComponent<FacepunchTransport>();

        SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyInvite += OnLobbyInvite;
        SteamMatchmaking.OnLobbyDataChanged += OnLobbyDataChanged;
        SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;

        CurrentUser = new Friend(SteamClient.SteamId);
    }

    void OnDestroy()
    {
        SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyInvite -= OnLobbyInvite;
        SteamMatchmaking.OnLobbyDataChanged -= OnLobbyDataChanged;
        SteamFriends.OnGameLobbyJoinRequested -= OnGameLobbyJoinRequested;

        if (NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
        NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
    }

    private void OnLobbyDataChanged(Lobby lobby)
    {
        CurrentLobby = lobby;
        LobbyDataChanged.Invoke(lobby);
    }

    void OnApplicationQuit() => Disconnect();

    // Update is called once per frame
    void Update()
    {

    }

    public async void StartHost(uint maxMembers)
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;

        NetworkManager.Singleton.StartHost();

        CurrentLobby = await SteamMatchmaking.CreateLobbyAsync((int)maxMembers);
    }

    public void InviteFriend()
    {
        SteamId currentLobbyId = CurrentLobby.Value.Id;
        SteamFriends.OpenGameInviteOverlay(currentLobbyId);
    }

    public void StartClient(SteamId id)
    {
        NetworkManager.Singleton.OnClientConnectedCallback += ClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += ClientDisconnected;

        transport.targetSteamId = id;

        CurrentUser = new Friend(SteamClient.SteamId);

        if (NetworkManager.Singleton.StartClient())
            Debug.Log("Client has joined!", this);
    }

    private void Disconnect()
    {
        CurrentLobby?.Leave();

        if (NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.Shutdown();
    }

    private Steamworks.ServerList.Internet GetInternetRequest()
    {
        var request = new Steamworks.ServerList.Internet();

        return request;
    }

    private async void OnGameLobbyJoinRequested(Lobby lobby, SteamId id)
    {
        await lobby.Join();

        bool isSame = lobby.Owner.Id.Equals(id);
        Debug.Log($"Owner: {lobby.Owner}");
        Debug.Log($"Id: {id}");
        Debug.Log($"IsSame: {isSame}", this);

        // StartClient(id);
    }

    private void OnLobbyInvite(Friend friend, Lobby lobby)
    {
        Debug.Log($"You got a invite from {friend.Name}", this);
    }

    private void OnLobbyMemberLeave(Lobby lobby, Friend friend)
    {

    }

    private void OnLobbyMemberJoined(Lobby lobby, Friend friend)
    {
        Debug.Log($"Lobby Member Joined, {friend.Name}");
        LobbyMemberJoined.Invoke(lobby, friend);
    }

    private void OnLobbyEntered(Lobby lobby)
    {
        Debug.Log($"OnLobbyEntered called. IsHost: {NetworkManager.Singleton.IsHost}");
        Debug.Log($"Lobby Owner: {lobby.Owner.Id}, Current User: {SteamClient.SteamId}");

        if (NetworkManager.Singleton.IsHost)
        {
            Debug.Log("User is host, not starting client");
            return;
        }

        CurrentLobby = lobby;
        LobbyEntered?.Invoke(lobby);

        Debug.Log($"About to start client with SteamId: {lobby.Owner.Id}");
        StartClient(lobby.Owner.Id);
    }

    private void OnLobbyCreated(Result result, Lobby lobby)
    {
        if (result != Result.OK)
        {
            Debug.Log($"Lobby couldn'y be created!, {result}", this);
            return;
        }

        lobby.SetFriendsOnly();
        lobby.SetData("name", "playroom");
        lobby.SetJoinable(true);

        OnHostCreated.Invoke(lobby.Id);

        Debug.Log("Lobby has been created");
    }

    private void OnServerStarted()
    {
        Debug.Log("Server started");
    }


    private void ClientConnected(ulong clientId)
    {
        Debug.Log($"I'm connected, clientId = {clientId}");
    }

    private void ClientDisconnected(ulong clientId)
    {
        Debug.Log($"I'm connected, clientId = {clientId}");

        NetworkManager.Singleton.OnClientDisconnectCallback -= ClientDisconnected;
        NetworkManager.Singleton.OnClientConnectedCallback -= ClientConnected;
    }



    private void OnClientConnectedCallback(ulong clientId)
    {
        Debug.Log($"Client connected, clientId={clientId}", this);
    }

    private void OnClientDisconnectCallback(ulong clientId)
    {
        Debug.Log($"Client Disconnected, clientId={clientId}", this);
    }
}
