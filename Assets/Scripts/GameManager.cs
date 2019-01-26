using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance = null;
    [HideInInspector]
    public NetworkManager NetworkManager;
    public NetworkMessageManager NetworkMessageManager;
    public MenuManager MenuManager;

    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
        StartCoroutine(Initialize());
    }

    private IEnumerator Initialize()
    {
        yield return StartCoroutine(InitializeNetwork());
    }
    
    private IEnumerator InitializeNetwork()
    {
        NetworkManager = GetComponent<NetworkManager>();
        NetworkMessageManager = GetComponent<NetworkMessageManager>();
        MenuManager = GetComponent<MenuManager>();
        yield return null;
    }
}
