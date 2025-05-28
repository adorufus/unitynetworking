using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class NetworkSpawner : NetworkBehaviour
{
    public GameObject playerPrefabs;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnLoadComplete += OnLoadComplete;
        }
    }

    private void OnLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        if (!IsServer) return;

        if (sceneName == "Main")
        {
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                if (!client.PlayerObject)
                {
                    var player = Instantiate(playerPrefabs, GetSpawnPos(), Quaternion.identity);
                    player.GetComponent<NetworkObject>().SpawnAsPlayerObject(client.ClientId);
                }
            }
        }
    }

    private Vector3 GetSpawnPos()
    {
        return new Vector3(transform.position.x + Random.Range(-5f, 5), 0f, transform.position.z + Random.Range(-5f, 5f));
    }
}
