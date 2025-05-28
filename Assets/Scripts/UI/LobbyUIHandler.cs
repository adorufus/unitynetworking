using System;
using Steamworks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyUIHandler : MonoBehaviour
{

    public Button hostGameBtn;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hostGameBtn.onClick.AddListener(OnHostGameClick);
    }

    void OnDestroy()
    {
        hostGameBtn.onClick.RemoveListener(OnHostGameClick);
        GameNetworkManager.Instance.OnHostCreated -= OnHostCreated;
    }

    private void OnHostGameClick()
    {
        GameNetworkManager.Instance.StartHost(10);
        GameNetworkManager.Instance.OnHostCreated += OnHostCreated;
    }

    private void OnHostCreated(SteamId id)
    {
        SceneManager.LoadScene("Lobby");
    }

    private void OnJoinGameClick()
    {
        ///handle on join game click
    }

    // Update is called once per frame
    void Update()
    {

    }
}
