using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject MainMenu;
    public GameObject Rooms;

    public void ShowMenu()
    {
        Rooms.SetActive(false);
        GameManager.Instance.NetworkManager.StopBroadcastClient();
        MainMenu.SetActive(true);
    }

    public void ShowRooms()
    {
        MainMenu.SetActive(false);
        GameManager.Instance.NetworkManager.StartBroadcastClient();
        Rooms.SetActive(true);
    }

    public void StartGame()
    {
        MainMenu.SetActive(false);
        GameManager.Instance.NetworkManager.StartBroadcasting();
    }

    public void JoinGame()
    {
        Rooms.SetActive(false);
        GameManager.Instance.NetworkManager.StopBroadcastClient();
    }
}
