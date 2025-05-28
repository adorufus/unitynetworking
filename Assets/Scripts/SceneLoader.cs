using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(LoadMainMenuScene());
    }

    IEnumerator LoadMainMenuScene()
    {
        yield return new WaitUntil(() => NetworkManager.Singleton != null);

        SceneManager.LoadScene("SampleScene");
    }
}
